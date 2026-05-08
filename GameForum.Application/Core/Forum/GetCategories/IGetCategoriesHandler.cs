using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Forum.GetCategories;

public interface IGetCategoriesHandler
{
    Task<Response<List<CategoryModel>>> HandleAsync(CancellationToken cancellationToken = default);
}
