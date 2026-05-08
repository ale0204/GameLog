using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.CreateThread;

public interface ICreateThreadHandler
{
    Task<Response<ThreadModel>> HandleAsync(ThreadModel model, CancellationToken cancellationToken = default);
}
