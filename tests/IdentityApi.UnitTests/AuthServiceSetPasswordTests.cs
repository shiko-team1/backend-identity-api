using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Application.Inputs;
using Application.Outputs;

public class AuthServiceSetPasswordTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly AuthService _authService;

    public AuthServiceSetPasswordTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

        _authService = new AuthService(_userManagerMock.Object, Mock.Of<SignInManager<IdentityUser>>());
    }

    [Fact]
    public async Task SetPasswordAsync_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new SetPasswordRequest("nonexistent@example.com", "NewPassword123!");
        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authService.SetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(SetPasswordStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task SetPasswordAsync_ShouldReturnAlreadyHasPassword_WhenUserAlreadyHasPassword()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com" };
        var request = new SetPasswordRequest(user.Email, "NewPassword123!");

        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.HasPasswordAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.SetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(SetPasswordStatus.AlreadyHasPassword, result.Status);
    }

    [Fact]
    public async Task SetPasswordAsync_ShouldReturnSuccess_WhenPasswordIsSetSuccessfully()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com" };
        var request = new SetPasswordRequest(user.Email, "NewPassword123!");

        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.HasPasswordAsync(user))
            .ReturnsAsync(false);
        _userManagerMock.Setup(um => um.AddPasswordAsync(user, request.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.SetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(SetPasswordStatus.Success, result.Status);
    }

    [Fact]
    public async Task SetPasswordAsync_ShouldReturnInvalidPassword_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com" };
        var request = new SetPasswordRequest(user.Email, "weak");

        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.HasPasswordAsync(user))
            .ReturnsAsync(false);
        _userManagerMock.Setup(um => um.AddPasswordAsync(user, request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        // Act
        var result = await _authService.SetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(SetPasswordStatus.InvalidPassword, result.Status);
        Assert.Equal("Password too weak.", result.ErrorMessage);
    }
}