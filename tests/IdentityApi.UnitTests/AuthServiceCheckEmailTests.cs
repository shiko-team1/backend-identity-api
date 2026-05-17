using Application.Outputs;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class AuthServiceCheckEmailTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly AuthService _authService;

    public AuthServiceCheckEmailTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<IdentityUser>>());

        _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task CheckEmailAsync_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        _userManagerMock
            .Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser)null);

        var result = await _authService.CheckEmailAsync("nonexistent@example.com", CancellationToken.None);

        Assert.Equal(EmailStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task CheckEmailAsync_ShouldReturnConfirmed_WhenEmailIsConfirmed()
    {
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = true };
        _userManagerMock
            .Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(um => um.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        var result = await _authService.CheckEmailAsync("test@example.com", CancellationToken.None);

        Assert.Equal(EmailStatus.Confirmed, result.Status);
    }

    [Fact]
    public async Task CheckEmailAsync_ShouldReturnNotConfirmed_WhenEmailIsNotConfirmed()
    {
        var user = new IdentityUser { Email = "test@example.com", EmailConfirmed = false };
        _userManagerMock
            .Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(um => um.IsEmailConfirmedAsync(user))
            .ReturnsAsync(false);

        var result = await _authService.CheckEmailAsync("test@example.com", CancellationToken.None);

        Assert.Equal(EmailStatus.NotConfirmed, result.Status);
    }
}