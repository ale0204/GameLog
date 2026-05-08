using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.CreatePost;

public class CreatePostHandler : ICreatePostHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public CreatePostHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<PostModel>> HandleAsync(PostModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<PostModel>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                response.ErrorMessage = "Post content cannot be empty";
                return response;
            }
            if (model.ThreadId == Guid.Empty)
            {
                response.ErrorMessage = "ThreadId is required";
                return response;
            }
            if (model.UserId == Guid.Empty)
            {
                response.ErrorMessage = "UserId is required";
                return response;
            }

            // 3. Domain rules
            var thread = await _unitOfWork.Threads.GetByIdAsync(model.ThreadId, cancellationToken);
            if (thread is null)
            {
                response.ErrorMessage = "Thread does not exist";
                return response;
            }
            if (thread.IsLocked)
            {
                response.ErrorMessage = "Thread is locked";
                return response;
            }

            // 4. Persist
            var entity = model.ToEntity();
            await _unitOfWork.Posts.AddAsync(entity, cancellationToken);

            thread.LastActivityAt = DateTime.UtcNow;
            _unitOfWork.Threads.Update(thread);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
