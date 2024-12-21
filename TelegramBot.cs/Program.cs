using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using Domain;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Game = Domain.Game;
using Random = System.Random;

class Program
{
    private static readonly string BotToken = "7231678321:AAGQCSjBIzW7b0oqMfp9jZJS93tXwpbHKGw";

    private static readonly ConcurrentDictionary<long, Player> Players = new();
    private static Game CurrentGame = new();
    private static Player CurrentPlayer = new();
    private static Domain.SecretSantaDbContext db = new();
    
    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(BotToken);
        
        Console.WriteLine("Бот запущен..");

        using var cts = new CancellationTokenSource();

        var receiver = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            HandleUpdateAsync, HadlleErrorAsync, receiver, cancellationToken: cts.Token);
        
        var botInfo = await botClient.GetMeAsync();
        Console.WriteLine($"Бот {botInfo.Username} готов к работе");
            
        Console.ReadKey();
        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        var chatId = message.Chat.Id;
        var userMessage = message.Text;

        if (userMessage.StartsWith("/start"))
        {
            await botClient.SendTextMessageAsync(chatId, "Добро пожаловать в игру, Тайны Санты!! Используйте /join чтобы зарегистрироваться", cancellationToken: cancellationToken);
            return;
        }

        if (userMessage.StartsWith("/join"))
        {
            if(Players.ContainsKey(chatId))
            {
                await botClient.SendTextMessageAsync(chatId, "Вы уже зарегистрированы.", cancellationToken: cancellationToken);
                return;
            }

            var playerName = message.Chat.Username ?? "Игрок";
            Players[chatId] = new Player { ChatId = chatId, Name = playerName };
            await botClient.SendTextMessageAsync(chatId, "Вы успешно зарегистрированы!", cancellationToken: cancellationToken);
            return;
        }
        
        if (userMessage.StartsWith("/addadmin"))
        {
            if (!CurrentPlayer.IsAdmin)
            {
                await botClient.SendTextMessageAsync(chatId, "Только администратор может добавлять новых администраторов", cancellationToken: cancellationToken);
            }
            
            if (CurrentGame.IsActive)
            {
                await botClient.SendTextMessageAsync(chatId, "Игра уже началась, вы не можете добавить администраторов.", cancellationToken: cancellationToken);
                return;
            }

            if (Players.ContainsKey(chatId))
            {
                Players[chatId].IsAdmin = true;
                await botClient.SendTextMessageAsync(chatId, "Вы назначены на должность администратора.", cancellationToken: cancellationToken);
            }
            
            if (!Players.ContainsKey(chatId))
            {
                Players[chatId].IsAdmin = true;
                await botClient.SendTextMessageAsync(chatId, "Вы уже находитесь на должности админа", cancellationToken: cancellationToken);
            }

            else
            {
                await botClient.SendTextMessageAsync(chatId, "Вы должны зарегистрироваться перед тем, как стать администратором.", cancellationToken: cancellationToken);
            }
            
            return;
        }

        if (userMessage.StartsWith("/setamount"))
        {
            if (!Players.TryGetValue(chatId, out var player) || !player.IsAdmin)
            {
                await botClient.SendTextMessageAsync(chatId, "Только администратор может установить сумму.", cancellationToken:cancellationToken);
                return;
            }

            var parts = userMessage.Split(' ');
            if (parts.Length == 3 && decimal.TryParse(parts[1], out var result))
            {
                CurrentGame.Amount = result;
                CurrentGame.Currency = parts[2];
                await botClient.SendTextMessageAsync(chatId, $"Сумма подарка установлена: {result} {parts[2]}", cancellationToken: cancellationToken);
            }

            else
            {
                await botClient.SendTextMessageAsync(chatId, "Используйте формат: /setamount <сумма> <валюта>.", cancellationToken: cancellationToken);
            }
            
            return;
        }

        if (userMessage.StartsWith("/start_game"))
        {
            if (!Players.TryGetValue(chatId, out var player) || !player.IsAdmin)
            {
                await botClient.SendTextMessageAsync(chatId, "Только администратор может начать игру.", cancellationToken: cancellationToken);
                return;
            }

            if (Players.Count < 2)
            {
                await botClient.SendTextMessageAsync(chatId, "Недостаточно игроков для игры.", cancellationToken: cancellationToken);
                return;
            }

            if (!CurrentGame.IsActive)
            {
                CurrentGame.IsActive = true;
                await db.SaveChangesAsync(cancellationToken);

                await botClient.SendTextMessageAsync(chatId, "Игра завершена. Теперь вы можете добавить новых администраторов", cancellationToken: cancellationToken);
            }

            CurrentGame.IsActive = true;
            CurrentGame.Players = Players.Values.ToList();

            var random = new Random();
            var shutteredPlayers = CurrentGame.Players.OrderBy(_ => random.Next()).ToList();

            for (int i = 0; i < shutteredPlayers.Count; i++)
            {
                var givver = shutteredPlayers[i];
                var reciver = shutteredPlayers[(i+1)%shutteredPlayers.Count];
                CurrentGame.Parties[givver.ChatId] = reciver.ChatId;

                await botClient.SendTextMessageAsync(givver.ChatId, $"$\"\ud83c\udf85 Тайна Санты: Вы дарите подарок {{receiver.Name}}! \ud83c\udf81\\n\" +\n  $\"Сумма подарка: {{CurrentGame.Amount}} {{CurrentGame.Currency}}.\"", cancellationToken: cancellationToken);
            }

            await botClient.SendTextMessageAsync(chatId, "Игра началась! Всем участникам отправлены сообщения.", cancellationToken: cancellationToken);
            return;
        }
        
        Console.WriteLine($"Получено сообщение: '{message.Text}' в чате {chatId}");

        await botClient.SendTextMessageAsync(chatId: chatId, text: $"Вы написали сообщение: {message.Text}", cancellationToken: cancellationToken);
    }

    private static Task HadlleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Ошибка Telegram API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        
        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    private static async Task CheckGameStateAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        using var dbContext = new SecretSantaDbContext();

        var currentGame = await dbContext.Games.FirstOrDefaultAsync(g => g.IsActive, cancellationToken);
        if (currentGame == null)
        {
            await botClient.SendTextMessageAsync(chatId, "Активная игра не идёт.", cancellationToken: cancellationToken);
            return;
        }

        if (currentGame.IsActive)
        {
            await botClient.SendTextMessageAsync(chatId, "Игра уже началась, изменения не доступны.", cancellationToken: cancellationToken);
        }

        await botClient.SendTextMessageAsync(chatId, "Вы можете добавлять новых администраторов", cancellationToken: cancellationToken);
    }
}