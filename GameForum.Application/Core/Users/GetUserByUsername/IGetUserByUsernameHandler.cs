using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Users.GetUserByUsername;

public interface IGetUserByUsernameHandler
{
    Task<Response<UserModel>> HandleAsync(string username, CancellationToken cancellationToken = default);
}
