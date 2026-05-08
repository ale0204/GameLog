namespace GameForum.Application.Common.Models;

public class UserBadgeModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BadgeId { get; set; }

    public DateTime AwardedAt { get; set; }

    public BadgeModel? Badge { get; set; }
}
