using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.CreateCategory;

public class CreateCategoryHandler : ICreateCategoryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public CreateCategoryHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // [Authorize(Roles = "Admin")]
    public async Task<Response<CategoryModel>> HandleAsync(CategoryModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<CategoryModel>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                response.ErrorMessage = "Category name cannot be empty";
                return response;
            }

            // 3. Persist
            var entity = model.ToEntity();
            await _unitOfWork.Categories.AddAsync(entity, cancellationToken);
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
