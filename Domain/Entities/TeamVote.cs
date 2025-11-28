using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TeamVote
{
    [Key]
    public int TeamVoteId { get; init; }

    [Required]
    public int TeamId { get; init; }

    [Required]
    public int Seat { get; init; }

    [Required]
    public bool IsApproved { get; init; }

    public Team Team { get; set; } = null!;
}
