using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.VotePost;

public class VotePostHandler : IVotePostHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public VotePostHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<VoteModel>> HandleAsync(VoteModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<VoteModel>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (model.PostId == Guid.Empty)
            {
                response.ErrorMessage = "PostId is required";
                return response;
            }
            if (model.UserId == Guid.Empty)
            {
                response.ErrorMessage = "UserId is required";
                return response;
            }

            // 3. Domain rules
            var post = await _unitOfWork.Posts.GetByIdAsync(model.PostId, cancellationToken);
            if (post is null)
            {
                response.ErrorMessage = "Post not found";
                return response;
            }
            if (post.IsDeleted)
            {
                response.ErrorMessage = "Cannot vote on a deleted post";
                return response;
            }

            var existing = await _unitOfWork.Votes.GetByUserAndPostAsync(model.UserId, model.PostId, cancellationToken);
            if (existing is not null)
            {
                if (existing.Value == model.Value)
                {
                    response.ErrorMessage = "You already cast this vote";
                    return response;
                }
                existing.Value = model.Value;
                _unitOfWork.Votes.Update(existing);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                // TODO: recalculate TrendingScore
                response.Data = existing.ToModel();
                return response;
            }

            // 4. Persist new vote
            var entity = model.ToEntity();
            await _unitOfWork.Votes.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            // TODO: recalculate TrendingScore

            response.Data = entity.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
