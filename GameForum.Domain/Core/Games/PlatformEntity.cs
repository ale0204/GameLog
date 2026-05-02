using GameForum.Domain.Common;

namespace GameForum.Domain.Core.Games;

public class PlatformEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
}
