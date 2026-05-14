using Application.Abstractions;
using Application.Inputs;
using Application.Outputs;
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
            return new SetPasswordResult(SetPasswordStatus.Success);
        }

        var error = result.Errors.FirstOrDefault()?.Description;
        return new SetPasswordResult(SetPasswordStatus.InvalidPassword, error ?? "Invalid password.");
    }
}
