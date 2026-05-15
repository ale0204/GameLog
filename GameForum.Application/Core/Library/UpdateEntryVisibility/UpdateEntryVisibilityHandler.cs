using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Library.UpdateEntryVisibility;

public class UpdateEntryVisibilityHandler : IUpdateEntryVisibilityHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public UpdateEntryVisibilityHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<UserGameModel>> HandleAsync(UpdateVisibilityCommand command, CancellationToken cancellationToken = default)
    {
        var response = new Response<UserGameModel>();
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
                response.ErrorMessage = "Only the owner can change visibility";
                return response;
            }

            entry.Visibility = command.Visibility;
            _unitOfWork.UserGames.Update(entry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = entry.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
