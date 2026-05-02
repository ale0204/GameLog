using GameForum.Domain.Common;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Social;

public class MessageEntity : BaseEntity
{
    public Guid SenderId { get; set; }
    public UserEntity Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public UserEntity Receiver { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }
}
