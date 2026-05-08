using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetThreadsByCategory;

public class GetThreadsByCategoryHandler : IGetThreadsByCategoryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetThreadsByCategoryHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<ThreadModel>>> HandleAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<ThreadModel>>();
        try
        {
            // 1. Auth check (public listing)
            if (categoryId == Guid.Empty)
            {
                response.ErrorMessage = "CategoryId is required";
                return response;
            }

            // TODO: order by TrendingScore once worker is registered; provisionally by CreatedAt desc
            var entities = await _unitOfWork.Threads.GetByCategoryIdAsync(categoryId, cancellationToken);
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
