using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Rules;
using Domain.Services;

namespace Application.UseCases.Games;
public sealed class StartGameFeature(IGameRepository gameRepository)
{
    public async Task ExecuteAsync(Game game)
    {
        if (game == null)
            throw new ArgumentException("Game not found.");
        if (game.Status != GameStatus.Lobby)
            throw new InvalidOperationException("Game is not in a state that can be started.");

        var players = game.Players.ToList();
        var playerCount = players.Count;

        if (playerCount < GameRules.MinPlayerCount)
            throw new InvalidOperationException($"Not enough players to start the game. Minimum is {GameRules.MinPlayerCount}.");

        RoleAssignmentService.AssignRoles(players);


        game.Status = GameStatus.InProgress;
        game.RoundCounter = 1;
        game.LeaderSeat = 1;

        await gameRepository.SaveChangesAsync()
            .ConfigureAwait(false);

    }

    

    

}
