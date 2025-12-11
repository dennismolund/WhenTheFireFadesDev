using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GamePlayerRepository(ApplicationDbContext db) : IGamePlayerRepository
{
    public async Task AddPlayerAsync(GamePlayer player)
    {
        await db.GamePlayers
            .AddAsync(player);
    }

    public async Task<int> GetNextAvailableSeatAsync(int gameId)
    {
        var takenSeats = await db.GamePlayers
            .AsNoTracking()
            .Where(gp => gp.GameId == gameId)
            .OrderBy(gp => gp.Seat)
            .Select(gp => gp.Seat)
            .ToListAsync();


        var expectedSeat = 1;

        foreach (var seat in takenSeats)
        {
            if (seat > expectedSeat)
            {
                break;
            }

            if (seat == expectedSeat)
            {
                expectedSeat++;
            }
        }

        return expectedSeat;
    }

    public void RemovePlayer(GamePlayer player)
    {
        db.GamePlayers.Remove(player);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
    
}
