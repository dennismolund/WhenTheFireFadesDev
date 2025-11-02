using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RoundRepository(ApplicationDbContext db) : IRoundRepository
{
    public async Task AddRoundAsync(Round round)
    {
        await db.Rounds.AddAsync(round)
            .ConfigureAwait(false);

    }

    public async Task UpdateRoundStatus(int roundId, RoundStatus status)
    {
        var round = await db.Rounds.FindAsync(roundId)
            .ConfigureAwait(false);
        
        if (round != null)
        {
            round.Status = status;
        }
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync()
            .ConfigureAwait(false);

    }

    public async Task<Round> GetCurrentRoundSnapshot(int gameId, int roundNumber)
    {
        try
        {
            return await db.Rounds
                .Include(r => r.Teams.Where(tp => tp.IsActive))
                    .ThenInclude(tp => tp.Votes)        
                .Include(r => r.Teams.Where(tp => tp.IsActive))
                    .ThenInclude(tp => tp.Members)   
                .SingleAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber)
                .ConfigureAwait(false);

        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Expected exactly one round for game {gameId} and round number {roundNumber}. but found none or many",
                ex
            );
        }
    }

}
