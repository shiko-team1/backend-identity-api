using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityApi.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public AuthEndpointsTests(IdentityApiFactory factory)
    {
        _client = factory.CreateDefaultClient(new Uri("https://localhost"));
    }

    [Fact]
    public async Task ProtectedAuthEndpoint_ReturnsUnauthorized_WhenApiKeyIsMissing()
    {
        var response = await _client.GetAsync("/api/auth/email-status?email=missing@example.com");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedAuthEndpoint_ReturnsUnauthorized_WhenApiKeyIsInvalid()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/email-status?email=missing@example.com");
        request.Headers.Add("X-API-KEY", "wrong-key");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserCanBeCreatedConfirmedAndLoggedIn_WhenApiKeyIsValid()
    {
        var email = $"student-{Guid.NewGuid():N}@example.com";
        const string password = "TestPassword123!";

        var createResponse = await _client.PostAsJsonAsync("/api/admin/users", new
        {
            email,
            role = "student"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdUser = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>(JsonOptions);
        Assert.NotNull(createdUser);
        Assert.False(string.IsNullOrWhiteSpace(createdUser.UserId));
        Assert.Equal(email, createdUser.Email);
        Assert.Equal("student", createdUser.Role);

        var notConfirmedResponse = await SendWithApiKeyAsync(HttpMethod.Get, $"/api/auth/email-status?email={Uri.EscapeDataString(email)}");

        Assert.Equal(HttpStatusCode.OK, notConfirmedResponse.StatusCode);
        var notConfirmed = await notConfirmedResponse.Content.ReadFromJsonAsync<EmailStatusResponse>(JsonOptions);
        Assert.NotNull(notConfirmed);
        Assert.False(notConfirmed.Confirmed);

        var setPasswordResponse = await SendWithApiKeyAsync(HttpMethod.Post, "/api/auth/gateway/set-password", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, setPasswordResponse.StatusCode);

        var confirmResponse = await SendWithApiKeyAsync(HttpMethod.Post, "/api/auth/confirm-email", new
        {
            email
        });

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var loginResponse = await SendWithApiKeyAsync(HttpMethod.Post, "/api/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginUser = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>(JsonOptions);
        Assert.NotNull(loginUser);
        Assert.Equal(createdUser.UserId, loginUser.Id);
        Assert.Equal(email, loginUser.Email);
        Assert.Contains("student", loginUser.Roles);
    }

    private async Task<HttpResponseMessage> SendWithApiKeyAsync(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-API-KEY", IdentityApiFactory.ApiKey);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await _client.SendAsync(request);
    }

    private sealed record CreateUserResponse(string? UserId, string? Email, string? Role);
    private sealed record EmailStatusResponse(bool Confirmed);
    private sealed record LoginUserResponse(string Id, string Email, string[] Roles);
}
