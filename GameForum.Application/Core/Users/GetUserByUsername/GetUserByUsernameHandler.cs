using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Users.GetUserByUsername;

public class GetUserByUsernameHandler : IGetUserByUsernameHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetUserByUsernameHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<UserModel>> HandleAsync(string username, CancellationToken cancellationToken = default)
    {
        var response = new Response<UserModel>();
        try
        {
            // 1. Auth check (public endpoint)
            if (string.IsNullOrWhiteSpace(username))
            {
                response.ErrorMessage = "Username is required";
                return response;
            }

            var entity = await _unitOfWork.Users.GetByUsernameAsync(username, cancellationToken);
            if (entity is null)
            {
                response.ErrorMessage = "User not found";
                return response;
            }

            response.Data = entity.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
