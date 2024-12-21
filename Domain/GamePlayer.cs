namespace Domain;

/// <summary>
/// TODO: Класс игроков игры
/// </summary>
public class GamePlayer
{
    /// <summary>
    /// TODO: Свойство id игрока в игре
    /// </summary>
    public int GamePlayerId { get; set; }
    /// <summary>
    /// TODO: Свойство id игры
    /// </summary>
    public int GameId { get; set; }
    /// <summary>
    /// TODO: Свойство класса игры
    /// </summary>
    public Game Game { get; set; }
    /// <summary>
    /// TODO: Свойство id игроков
    /// </summary>
    public int PlayerId { get; set; }
    /// <summary>
    /// TODO: Свойство класса игроков
    /// </summary>
    public Player Player { get; set; }
    /// <summary>
    /// TODO: Свойство отправленного id
    /// </summary>
    public int? ReciverId { get; set; }
    /// <summary>
    /// TODO: Свойство отправленного id игроков
    /// </summary>
    public Player? Reciver { get; set; }
}