using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class GamePlayer
{
    [Key]
    public int GamePlayerId { get; init; }

    [Required]
    public int GameId { get; init; }

    public int? TempUserId { get; init; }
    
    public string? UserId { get; init; }

    [MaxLength(40)]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    public int Seat { get; set; }

    [Required]
    public PlayerRole Role { get; set; } = PlayerRole.Human;
    
    [Required]
    public bool IsConnected { get; set; } = true;
    
    public Game Game { get; set; } = default!;
    
    
}

