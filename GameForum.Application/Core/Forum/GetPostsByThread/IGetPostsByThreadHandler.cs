using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetPostsByThread;

public interface IGetPostsByThreadHandler
{
    Task<Response<List<PostModel>>> HandleAsync(Guid threadId, CancellationToken cancellationToken = default);
}
