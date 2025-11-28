using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class MissionVote
{
    [Key]
    public int MissionVoteId { get; init; }

    [Required]
    public int RoundId { get; init; }

    [Required]
    public int Seat { get; init; }

    [Required]
    public bool IsSuccess { get; init; }

    public Round Round { get; init; } = null!;
}
