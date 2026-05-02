namespace GameForum.Domain.Common.Exceptions;

public class FriendshipAlreadyExistsException : DomainException
{
    public FriendshipAlreadyExistsException()
        : base("A friendship between these users already exists.") { }
}
