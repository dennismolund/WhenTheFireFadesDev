using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;

namespace Application.UseCases.Rounds;
public sealed class CreateRoundFeature(IRoundRepository roundRepository)
{
    public async Task ExecuteAsync(Game game, int roundNumber, int leaderSeat)
    {
        var playerCount = game.Players.Count;
        var teamSize = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        var round = new Round
        {
            GameId = game.GameId,
            RoundNumber = roundNumber,
            LeaderSeat = leaderSeat,
            TeamSize = teamSize,
            Status = RoundStatus.TeamSelection,
            Result = RoundResult.Unknown,
        };

        await roundRepository.AddRoundAsync(round)
            .ConfigureAwait(false);

        await roundRepository.SaveChangesAsync()
            .ConfigureAwait(false);
    }

    
}
