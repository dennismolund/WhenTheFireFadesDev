using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TeamMember
{
    [Key]
    public int TeamMemberId { get; init; }

    [Required]
    public int TeamId { get; init; }

    [Required]
    public int Seat { get; init; }

    public Team Team { get; set; } = null!;
}
