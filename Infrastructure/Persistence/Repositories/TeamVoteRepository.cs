using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeamVoteRepository(ApplicationDbContext db) : ITeamVoteRepository
{
    public async Task AddTeamVoteAsync(TeamVote teamVote)
    {
        await db.TeamVotes.AddAsync(teamVote)
            .ConfigureAwait(false);
    }

    public async Task<List<TeamVote>> GetByTeamAsync(int teamId)
    {
        return await db.TeamVotes
            .Where(v => v.TeamId == teamId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync()
            .ConfigureAwait(false);

    }
}