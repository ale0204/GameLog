namespace GameForum.Application.Common.Models;

public class BadgeModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    public string Condition { get; set; } = string.Empty;
}
