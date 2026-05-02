namespace GameForum.Domain.Common.Exceptions;

public class InvalidRatingException : DomainException
{
    public InvalidRatingException(int rating)
        : base($"Rating must be between 1 and 10. Got: {rating}.") { }
}
