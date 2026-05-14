using Application.Inputs;
using Application.Outputs;

namespace Application.Abstractions;

public interface IUserAdminService
{
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<DeleteUserResult> DeleteUserByIdAsync(string id, CancellationToken cancellationToken);
}
