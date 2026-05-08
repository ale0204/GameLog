using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.CreateCategory;

public interface ICreateCategoryHandler
{
    Task<Response<CategoryModel>> HandleAsync(CategoryModel model, CancellationToken cancellationToken = default);
}
