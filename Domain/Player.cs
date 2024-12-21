using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

/// <summary>
/// TODO: Класс игрок
/// </summary>
[Table("Player")]
public class Player
{
    /// <summary>
    /// TODO: Свойство id игрока
    /// </summary>
    public int PlayerId { get; set; }
    
    /// <summary>
    /// TODO: Свойство id чата
    /// </summary>
    public long ChatId { get; set; }
    
    /// <summary>
    /// TODO: Свойство имени
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// TODO: Свойство для проверки прав администратора
    /// </summary>
    public bool IsAdmin { get; set; }
}