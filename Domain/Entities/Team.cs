using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Team
{
    [Key]
    public int TeamId { get; init; }

    [Required]
    public int RoundId { get; init; }

    [Required]
    public bool IsActive { get; set; } = true;
    
    public Round Round { get; init; } = null!;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamVote> Votes { get; set; } = new List<TeamVote>();
}
