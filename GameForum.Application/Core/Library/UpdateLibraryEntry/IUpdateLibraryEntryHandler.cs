using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Library.UpdateLibraryEntry;

public interface IUpdateLibraryEntryHandler
{
    Task<Response<UserGameModel>> HandleAsync(UserGameModel model, CancellationToken cancellationToken = default);
}
