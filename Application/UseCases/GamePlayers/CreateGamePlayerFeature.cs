using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.GamePlayers;
public sealed class CreateGamePlayerFeature(IGamePlayerRepository gamePlayerRepository)
{
    public async Task ExecuteAsync(Game game, int creatorTempUserId, string? creatorUsername = null, string? userId = null)
    {
        var nextSeat = await gamePlayerRepository.GetNextAvailableSeatAsync(game.GameId)
            .ConfigureAwait(false);
            

        var player = new GamePlayer
        {
            GameId = game.GameId,
            TempUserId = creatorTempUserId,
            UserId = userId,
            Nickname = creatorUsername ?? $"Player#{creatorTempUserId}",
            Seat = nextSeat,
            Role = PlayerRole.Human,
            IsConnected = true,
        };

        await gamePlayerRepository.AddPlayerAsync(player)
            .ConfigureAwait(false);
        
        await gamePlayerRepository.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}


