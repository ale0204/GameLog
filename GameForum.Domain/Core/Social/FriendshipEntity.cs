using GameForum.Domain.Common;
using GameForum.Domain.Core.Social.Enums;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Social;

public class FriendshipEntity : BaseEntity
{
    public Guid SenderId { get; set; }
    public UserEntity Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public UserEntity Receiver { get; set; } = null!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
}
