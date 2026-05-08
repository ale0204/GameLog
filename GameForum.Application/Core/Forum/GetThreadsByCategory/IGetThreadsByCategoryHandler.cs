using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetThreadsByCategory;

public interface IGetThreadsByCategoryHandler
{
    Task<Response<List<ThreadModel>>> HandleAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
