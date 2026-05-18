using System;
using System.Collections.Generic;
using System.Text;
using Application.Inputs;
using Application.Outputs;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        Task<EmailCheckResult> CheckEmailAsync(string email, CancellationToken cancellationToken);
        Task<ConfirmEmailResult> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
        Task<SetPasswordResult> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken);
        Task<ChangePasswordResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
        Task<VerifyPasswordResult> VerifyPasswordAsync(VerifyPasswordRequest request, CancellationToken cancellationToken);
    }
}
