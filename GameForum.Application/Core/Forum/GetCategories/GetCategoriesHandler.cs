using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetCategories;

public class GetCategoriesHandler : IGetCategoriesHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetCategoriesHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<CategoryModel>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var response = new Response<List<CategoryModel>>();
        try
        {
            // 1. Auth check (public listing)
            var entities = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
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
