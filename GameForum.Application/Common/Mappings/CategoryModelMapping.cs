using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Forum;

namespace GameForum.Application.Common.Mappings;

public static class CategoryModelMapping
{
    public static CategoryModel ToModel(this CategoryEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        IconUrl = entity.IconUrl,
        SortOrder = entity.SortOrder,
        ThreadsCount = entity.Threads.Count,
        PostsCount = entity.Threads.SelectMany(t => t.Posts).Count(p => !p.IsDeleted)
    };

    public static CategoryEntity ToEntity(this CategoryModel model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        Name = model.Name,
        Description = model.Description,
        IconUrl = model.IconUrl,
        SortOrder = model.SortOrder
    };

    public static List<CategoryModel> ToModelList(this IEnumerable<CategoryEntity> entities) =>
        entities.Select(e => e.ToModel()).ToList();
}
