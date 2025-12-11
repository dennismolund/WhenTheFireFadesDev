using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.UseCases.Games;
public class CreateGameFeature(IGameRepository gameRepository)
{
    public async Task<Game> ExecuteAsync()
    {
        var game = new Game
        {
            ConnectionCode = GameCode.GenerateCode(),
            LeaderSeat = 1,
            Status = GameStatus.Lobby,
            GameWinner = GameResult.Unknown,
            RoundCounter = 0,
            SuccessCount = 0,
            SabotageCount = 0,
        };

        await gameRepository.AddGameAsync(game);

        await gameRepository.SaveChangesAsync();


        return game;
    }


}
        