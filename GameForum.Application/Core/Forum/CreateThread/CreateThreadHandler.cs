using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.CreateThread;

public class CreateThreadHandler : ICreateThreadHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public CreateThreadHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<ThreadModel>> HandleAsync(ThreadModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<ThreadModel>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                response.ErrorMessage = "Thread title cannot be empty";
                return response;
            }
            if (model.UserId == Guid.Empty)
            {
                response.ErrorMessage = "UserId is required";
                return response;
            }
            if (model.CategoryId == Guid.Empty)
            {
                response.ErrorMessage = "CategoryId is required";
                return response;
            }

            // 3. Domain rules
            var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId, cancellationToken);
            if (category is null)
            {
                response.ErrorMessage = "Category does not exist";
                return response;
            }

            // 4. Persist
            var entity = model.ToEntity();
            entity.LastActivityAt = DateTime.UtcNow;
            await _unitOfWork.Threads.AddAsync(entity, cancellationToken);
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
