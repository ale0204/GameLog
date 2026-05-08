using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Library.GetLibraryByUser;

public record GetLibraryByUserQuery(Guid OwnerId, Guid? RequesterId);

public interface IGetLibraryByUserHandler
{
    Task<Response<List<UserGameModel>>> HandleAsync(GetLibraryByUserQuery query, CancellationToken cancellationToken = default);
}
