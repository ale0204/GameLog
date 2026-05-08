using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.VotePost;

public interface IVotePostHandler
{
    Task<Response<VoteModel>> HandleAsync(VoteModel model, CancellationToken cancellationToken = default);
}
