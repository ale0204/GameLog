using GameForum.Application.Common.Errors;

namespace GameForum.Application.Core.Forum.DeletePost;

public record DeletePostCommand(Guid PostId, Guid RequesterId);

public interface IDeletePostHandler
{
    Task<Response<bool>> HandleAsync(DeletePostCommand command, CancellationToken cancellationToken = default);
}
