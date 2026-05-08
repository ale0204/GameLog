using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Library.AddToLibrary;

public interface IAddToLibraryHandler
{
    Task<Response<UserGameModel>> HandleAsync(UserGameModel model, CancellationToken cancellationToken = default);
}
