namespace Domain;

/// <summary>
/// TODO: Класс для игры
/// </summary>
public class Game
{
    /// <summary>
    /// TODO: Свойсто id игры
    /// </summary>
    public int GameId { get; set; }
    /// <summary>
    /// TODO: Свойство списка игроков
    /// </summary>
    public List<Player> Players { get; set; }
    /// <summary>
    /// TODO: Свойство суммы
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// TODO: Свойство валюты
    /// </summary>
    public string Currency { get; set; }
    /// <summary>
    /// TODO: Свойство словаря суммы и валюты 
    /// </summary>
    public Dictionary<long, long> Parties { get; set; } = new();
    /// <summary>
    /// TODO: Свойство активной игры
    /// </summary>
    public bool IsActive { get; set; } = false;
}