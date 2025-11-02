using Domain.Entities;

namespace Application.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByCodeAsync(string code);
    Task<Game?> GetByCodeWithPlayersAsync(string code);
    Task<Game?> GetByCodeWithPlayersAndRoundsAsync(string code);
    Task AddGameAsync(Game game);
    void DeleteGame(Game game);
    Task SaveChangesAsync();
}
