using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Messages.GetConversation;

public class GetConversationHandler : IGetConversationHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetConversationHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<MessageModel>>> HandleAsync(GetConversationQuery query, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<MessageModel>>();
        try
        {
            // 1. Auth check (caller must be UserA or UserB — placeholder)
            if (query.UserA == Guid.Empty)
            {
                response.ErrorMessage = "UserA is required";
                return response;
            }
            if (query.UserB == Guid.Empty)
            {
                response.ErrorMessage = "UserB is required";
                return response;
            }
            if (query.UserA == query.UserB)
            {
                response.ErrorMessage = "UserA and UserB must differ";
                return response;
            }

            var messages = await _unitOfWork.Messages.GetConversationAsync(query.UserA, query.UserB, cancellationToken);
            response.Data = messages.ToModelList();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
