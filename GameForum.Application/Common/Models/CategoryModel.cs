namespace GameForum.Application.Common.Models;

public class CategoryModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }

    public int ThreadsCount { get; set; }

    public int PostsCount { get; set; }
}
