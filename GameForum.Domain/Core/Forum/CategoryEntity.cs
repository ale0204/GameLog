using GameForum.Domain.Common;

namespace GameForum.Domain.Core.Forum;

public class CategoryEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }

    public ICollection<ThreadEntity> Threads { get; set; } = new List<ThreadEntity>();
}
