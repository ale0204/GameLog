using GameForum.Domain.Core.Social.Enums;

namespace GameForum.Application.Common.Models;

public class FriendshipModel
{
    public Guid Id { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

    public DateTime CreatedAt { get; set; }
}
