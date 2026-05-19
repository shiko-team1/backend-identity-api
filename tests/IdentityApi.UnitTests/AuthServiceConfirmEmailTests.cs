using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Application.Inputs;
using Application.Outputs;

public class AuthServiceConfirmEmailTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly AuthService _authService;

    public AuthServiceConfirmEmailTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

        _authService = new AuthService(_userManagerMock.Object, IdentityTestHelpers.CreateSignInManagerMock(_userManagerMock.Object).Object);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new ConfirmEmailRequest("nonexistent@example.com");
        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ConfirmEmailStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnAlreadyConfirmed_WhenEmailIsAlreadyConfirmed()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = true };
        var request = new ConfirmEmailRequest(user.Email);

        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ConfirmEmailStatus.AlreadyConfirmed, result.Status);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnConfirmed_WhenEmailIsSuccessfullyConfirmed()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = false };
        var request = new ConfirmEmailRequest(user.Email);

        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ConfirmEmailStatus.Confirmed, result.Status);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnError_WhenUpdateFails()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = false };
        var request = new ConfirmEmailRequest(user.Email);

        _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed." }));

        // Act
        var result = await _authService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(ConfirmEmailStatus.Error, result.Status);
        Assert.Equal("Could not confirm email.", result.ErrorMessage);
    }
}
