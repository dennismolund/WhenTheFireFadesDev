using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public abstract class BaseVote
{
    [Required]
    public int Seat { get; init; }
}