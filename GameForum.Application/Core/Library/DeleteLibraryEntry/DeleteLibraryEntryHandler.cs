using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;

namespace GameForum.Application.Core.Library.DeleteLibraryEntry;

public class DeleteLibraryEntryHandler : IDeleteLibraryEntryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public DeleteLibraryEntryHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<bool>> HandleAsync(DeleteLibraryEntryCommand command, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            // 1. Auth check
            if (command.EntryId == Guid.Empty)
            {
                response.ErrorMessage = "Entry Id is required";
                return response;
            }

            var entry = await _unitOfWork.UserGames.GetByIdAsync(command.EntryId, cancellationToken);
            if (entry is null)
            {
                response.ErrorMessage = "Library entry not found";
                return response;
            }
            if (entry.UserId != command.RequesterId)
            {
                response.ErrorMessage = "Only the owner can delete this library entry";
                return response;
            }

            // Hard delete — library entries don't need soft delete
            _unitOfWork.UserGames.Delete(entry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
