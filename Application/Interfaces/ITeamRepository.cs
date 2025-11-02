using Domain.Entities;

namespace Application.Interfaces;

public interface ITeamRepository
{
    Task AddTeamAsync(Team team);
    Task<Team> GetByRoundIdAsync(int roundId);
    Task<Team> GetActiveByRoundIdAsync(int roundId);
    Task SaveChangesAsync();
}
