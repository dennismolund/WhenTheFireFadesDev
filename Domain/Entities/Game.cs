using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Game
{
    [Key]
    public int GameId { get; init; }

    [Required]
    [MaxLength(10)]
    public string ConnectionCode { get; init; } = default!;

    [Required]
    public int LeaderSeat { get; set; }

    [Required]
    public GameStatus Status { get; set; } = GameStatus.Lobby;

    [Required]
    public GameResult GameWinner { get; set; } = GameResult.Unknown;

    [Required]
    public int RoundCounter { get; set; } = 0;

    [Required]
    public int SuccessCount { get; set; } = 0;

    [Required]
    public int SabotageCount { get; set; } = 0;

    [Required]
    public int ConsecutiveRejectedProposals { get; set; } = 0;

    public ICollection<GamePlayer> Players { get; set; } = new List<GamePlayer>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
