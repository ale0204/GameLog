using GameForum.Application.Common.Errors;

namespace GameForum.Application.Core.Library.DeleteLibraryEntry;

public record DeleteLibraryEntryCommand(Guid EntryId, Guid RequesterId);

public interface IDeleteLibraryEntryHandler
{
    Task<Response<bool>> HandleAsync(DeleteLibraryEntryCommand command, CancellationToken cancellationToken = default);
}
