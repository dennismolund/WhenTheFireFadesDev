using Domain.Entities;
using Application.UseCases.GamePlayers;
using Application.UseCases.Games;
using Application.UseCases.Rounds;


namespace Application.Services;

public sealed class GameOrchestrator(
    CreateGameFeature createGameFeature,
    CreateGamePlayerFeature createGamePlayerFeature,
    StartGameFeature startGameFeature,
    CreateRoundFeature createRoundFeature
    )
{
    public async Task<Game> CreateGameAsync()
    {
        return await createGameFeature.ExecuteAsync()
            .ConfigureAwait(false);

    }

    public async Task CreateGamePlayerAsync(Game game, int creatorTempUserId, string? creatorUsername = null, string? userId = null)
    {
        await createGamePlayerFeature.ExecuteAsync(game, creatorTempUserId, creatorUsername, userId)
            .ConfigureAwait(false);
    }

    public async Task StartGameAsync(Game game)
    {
        await startGameFeature.ExecuteAsync(game)
            .ConfigureAwait(false);

        await createRoundFeature.ExecuteAsync(game, game.RoundCounter, game.LeaderSeat)
            .ConfigureAwait(false);
    }

    public async Task CreateRoundAsync(Game game, int roundNumber, int leaderSeat)
    {
        await createRoundFeature.ExecuteAsync(game, roundNumber, leaderSeat)
            .ConfigureAwait(false);
    }
}
