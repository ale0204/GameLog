using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Games.GetGameById;

public interface IGetGameByIdHandler
{
    Task<Response<GameModel>> HandleAsync(Guid gameId, CancellationToken cancellationToken = default);
}
