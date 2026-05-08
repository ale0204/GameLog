using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Users.Enums;

namespace GameForum.Application.Core.Library.UpdateEntryVisibility;

public record UpdateVisibilityCommand(Guid EntryId, Visibility Visibility, Guid RequesterId);

public interface IUpdateEntryVisibilityHandler
{
    Task<Response<UserGameModel>> HandleAsync(UpdateVisibilityCommand command, CancellationToken cancellationToken = default);
}
