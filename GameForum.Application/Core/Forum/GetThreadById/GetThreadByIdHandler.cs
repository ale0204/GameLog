using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetThreadById;

public class GetThreadByIdHandler : IGetThreadByIdHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetThreadByIdHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<ThreadModel>> HandleAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        var response = new Response<ThreadModel>();
        try
        {
            // 1. Auth check (public)
            if (threadId == Guid.Empty)
            {
                response.ErrorMessage = "ThreadId is required";
                return response;
            }

            var entity = await _unitOfWork.Threads.GetByIdWithDetailsAsync(threadId, cancellationToken);
            if (entity is null)
            {
                response.ErrorMessage = "Thread not found";
                return response;
            }

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
