using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GameRepository(ApplicationDbContext db) : IGameRepository
{
    public async Task AddGameAsync(Game game)
    {
        await db.Games.AddAsync(game);
    }

    public void DeleteGame(Game game)
    {
        db.Games.Remove(game);
    }

    public async Task<Game?> GetByCodeAsync(string code)
    {
        return await db.Games
            .AsNoTracking()
            .Where(g => g.ConnectionCode == code &&
                        (g.Status == GameStatus.Lobby || g.Status == GameStatus.InProgress))
            .SingleOrDefaultAsync();

    }

    public async Task<Game?> GetByCodeWithPlayersAsync(string code)
    {
        return await db.Games
            .Where(g => g.ConnectionCode == code && 
                        (g.Status == GameStatus.Lobby || g.Status == GameStatus.InProgress))            
            .Include(g => g.Players)
            .SingleOrDefaultAsync();

    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();

    }

    public async Task<Game?> GetByCodeWithPlayersAndRoundsAsync(string code)
    {
        return await db.Games
            .Where(g => g.ConnectionCode == code &&
                        (g.Status == GameStatus.Lobby || g.Status == GameStatus.InProgress))
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .SingleOrDefaultAsync();

    }
}
