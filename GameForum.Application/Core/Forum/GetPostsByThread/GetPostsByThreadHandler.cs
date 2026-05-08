using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetPostsByThread;

public class GetPostsByThreadHandler : IGetPostsByThreadHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetPostsByThreadHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<PostModel>>> HandleAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<PostModel>>();
        try
        {
            // 1. Auth check (public)
            if (threadId == Guid.Empty)
            {
                response.ErrorMessage = "ThreadId is required";
                return response;
            }

            var entities = await _unitOfWork.Posts.GetByThreadIdAsync(threadId, cancellationToken);
            response.Data = entities.ToModelList();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
