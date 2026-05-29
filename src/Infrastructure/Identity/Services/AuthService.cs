using Application.Abstractions;
using Application.Inputs;
using Application.Outputs;
using Application.Outputs.Statuses;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public sealed class AuthService(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager) : IAuthService
{
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new LoginResult(LoginStatus.UserNotFound);
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return new LoginResult(LoginStatus.EmailNotConfirmed);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return new LoginResult(LoginStatus.InvalidCredentials);
        }

        var roles = await userManager.GetRolesAsync(user);
        var loginUser = new LoginUser(user.Id, user.Email ?? string.Empty, roles.ToArray());

        return new LoginResult(LoginStatus.Success, loginUser);
    }

    public async Task<EmailCheckResult> CheckEmailAsync(string email, CancellationToken cancellationToken)
    {
        email = email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new EmailCheckResult(EmailStatus.UserNotFound);
        }

        var confirmed = await userManager.IsEmailConfirmedAsync(user);
        return new EmailCheckResult(confirmed ? EmailStatus.Confirmed : EmailStatus.NotConfirmed);
    }

    public async Task<ConfirmEmailResult> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new ConfirmEmailResult(ConfirmEmailStatus.UserNotFound);
        }

        if (await userManager.IsEmailConfirmedAsync(user))
        {
            return new ConfirmEmailResult(ConfirmEmailStatus.AlreadyConfirmed);
        }

        user.EmailConfirmed = true;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? new ConfirmEmailResult(ConfirmEmailStatus.Confirmed)
            : new ConfirmEmailResult(ConfirmEmailStatus.Error, "Could not confirm email.");
    }

    public async Task<SetPasswordResult> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new SetPasswordResult(SetPasswordStatus.UserNotFound);
        }

        if (await userManager.HasPasswordAsync(user))
        {
            return new SetPasswordResult(SetPasswordStatus.AlreadyHasPassword);
        }

        var result = await userManager.AddPasswordAsync(user, request.Password);
        if (result.Succeeded)
        {
            return new SetPasswordResult(SetPasswordStatus.Success, UserId: user.Id);
        }

        var error = result.Errors.FirstOrDefault()?.Description;
        return new SetPasswordResult(SetPasswordStatus.InvalidPassword, error ?? "Invalid password.");
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new ChangePasswordResult(ChangePasswordStatus.UserNotFound);
        }

        if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            return new ChangePasswordResult(ChangePasswordStatus.InvalidCurrentPassword);
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            return new ChangePasswordResult(ChangePasswordStatus.Success);
        }

        var error = result.Errors.FirstOrDefault()?.Description;
        return new ChangePasswordResult(ChangePasswordStatus.InvalidNewPassword, error ?? "Invalid new password.");
    }

    public async Task<VerifyPasswordResult> VerifyPasswordAsync(VerifyPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new VerifyPasswordResult(VerifyPasswordStatus.UserNotFound);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        return result.Succeeded
            ? new VerifyPasswordResult(VerifyPasswordStatus.Valid)
            : new VerifyPasswordResult(VerifyPasswordStatus.InvalidPassword);
    }
}
