using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.CreatePost;

public interface ICreatePostHandler
{
    Task<Response<PostModel>> HandleAsync(PostModel model, CancellationToken cancellationToken = default);
}
