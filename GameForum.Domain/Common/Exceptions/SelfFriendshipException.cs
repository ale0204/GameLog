namespace GameForum.Domain.Common.Exceptions;

public class SelfFriendshipException : DomainException
{
    public SelfFriendshipException()
        : base("A user cannot send a friend request to themselves.") { }
}
