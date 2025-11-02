using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GameRepository(ApplicationDbContext db) : IGameRepository
{
    public async Task AddGameAsync(Game game)
    {
        await db.Games.AddAsync(game)
            .ConfigureAwait(false);
    }

    public void DeleteGame(Game game)
    {
        db.Games.Remove(game);
    }

    public async Task<Game?> GetByCodeAsync(string code)
    {
        return await db.Games
            .Where(g => g.ConnectionCode == code && 
                        (g.Status == GameStatus.Lobby || g.Status == GameStatus.InProgress))
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);

    }

    public async Task<Game?> GetByCodeWithPlayersAsync(string code)
    {
        return await db.Games
            .Where(g => g.ConnectionCode == code && 
                        (g.Status == GameStatus.Lobby || g.Status == GameStatus.InProgress))            
            .Include(g => g.Players)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);

    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync()
            .ConfigureAwait(false);

    }

    public async Task<Game?> GetByCodeWithPlayersAndRoundsAsync(string code)
    {
        return await db.Games
            .Where(g => g.ConnectionCode == code && 
                        (g.Status == GameStatus.Lobby || g.Status == GameStatus.InProgress))            
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .SingleOrDefaultAsync(g => g.ConnectionCode == code)
            .ConfigureAwait(false);

    }
}
