using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;

namespace GameForum.Application.Core.Forum.DeletePost;

public class DeletePostHandler : IDeletePostHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public DeletePostHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<bool>> HandleAsync(DeletePostCommand command, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (command.PostId == Guid.Empty)
            {
                response.ErrorMessage = "PostId is required";
                return response;
            }

            // 3. Domain rules
            var post = await _unitOfWork.Posts.GetByIdAsync(command.PostId, cancellationToken);
            if (post is null)
            {
                response.ErrorMessage = "Post not found";
                return response;
            }
            if (post.IsDeleted)
            {
                response.ErrorMessage = "Post is already deleted";
                return response;
            }
            if (post.UserId != command.RequesterId)
            {
                // TODO: also allow moderators / admins
                response.ErrorMessage = "Only the author can delete this post";
                return response;
            }

            // 4. Soft delete
            post.IsDeleted = true;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
