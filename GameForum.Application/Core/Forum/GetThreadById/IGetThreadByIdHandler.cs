using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetThreadById;

public interface IGetThreadByIdHandler
{
    Task<Response<ThreadModel>> HandleAsync(Guid threadId, CancellationToken cancellationToken = default);
}
