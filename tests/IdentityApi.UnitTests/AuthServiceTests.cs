using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Application.Inputs;
using Application.Outputs;
using Microsoft.AspNetCore.Http;

public class AuthServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
            null, null, null, null);

        _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@example.com", "password");
        _userManagerMock.Setup(um => um.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        Assert.Equal(LoginStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnEmailNotConfirmed_WhenEmailIsNotConfirmed()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = false };
        var loginRequest = new LoginRequest(user.Email, "password");

        _userManagerMock.Setup(um => um.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.IsEmailConfirmedAsync(user))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        Assert.Equal(LoginStatus.EmailNotConfirmed, result.Status);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnInvalidCredentials_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = true };
        var loginRequest = new LoginRequest(user.Email, "wrongpassword");

        _userManagerMock.Setup(um => um.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);
        _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new IdentityUser { Id = "1", Email = "test@example.com", EmailConfirmed = true };
        var loginRequest = new LoginRequest(user.Email, "correctpassword");

        _userManagerMock.Setup(um => um.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);
        _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new[] { "User" });

        // Act
        var result = await _authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.NotNull(result.User);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.Email, result.User.Email);
    }
}