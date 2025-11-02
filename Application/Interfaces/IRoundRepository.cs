using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IRoundRepository
{
    Task AddRoundAsync(Round round);
    Task SaveChangesAsync();
    Task UpdateRoundStatus(int roundId, RoundStatus status);
    Task<Round> GetCurrentRoundSnapshot(int gameId, int roundNumber);
}
