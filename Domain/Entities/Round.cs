using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Round
{
    [Key]
    public int RoundId { get; init; }

    [Required]
    public int GameId { get; init; }

    [Required]
    public int RoundNumber { get; init; }

    [Required]
    public int LeaderSeat { get; set; }

    [Required]
    public RoundStatus Status { get; set; } = RoundStatus.TeamSelection;

    public RoundResult? Result { get; set; } = RoundResult.Unknown;

    [Required]
    public int TeamSize { get; init; }
    
    public Game Game { get; init; } = null!;

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<MissionVote> MissionVotes { get; set; } = new List<MissionVote>();
}
