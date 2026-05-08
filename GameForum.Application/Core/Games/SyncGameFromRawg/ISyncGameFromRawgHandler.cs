using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Games.SyncGameFromRawg;

public interface ISyncGameFromRawgHandler
{
    Task<Response<GameModel>> HandleAsync(int rawgId, CancellationToken cancellationToken = default);
}
