using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MissionVoteRepository (ApplicationDbContext db) : IMissionVoteRepository 
{
    public async Task AddMissionVoteAsync(MissionVote missionVote)
    {
        await db.AddAsync(missionVote);

    }

    public async Task<List<MissionVote>> GetByRoundIdAsync(int roundId)
    {
        return await db.MissionVotes
            .Where(v => v.RoundId == roundId)
            .ToListAsync();

    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();

    }
}
