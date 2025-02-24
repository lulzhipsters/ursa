using System.Net;
using System.Net.Http.Json;
using Ursa.API;

namespace Ursa.Tests;

public class GetAccessTokenTests : IClassFixture<ApplicationFixture>, IAsyncLifetime
{
    private readonly ApplicationFactory _appFactory;

    public GetAccessTokenTests(ApplicationFixture fixture)
    {
        _appFactory = fixture.AppFactory!;
    }

    [Fact]
    public async Task API_WhenUserHeader_ReturnsTokens()
    {
        // Arrange
        var user = "User123";
        var client = _appFactory.CreateClient();
        client.DefaultRequestHeaders.Add(Headers.XUser, user);

        var expectedToken = await client.CreateTestToken(user);

        // Act
        var tokensResponse = await client.GetAsync(APIRoutes.GetUserTokens);
        var tokensContent = await tokensResponse.Content.ReadFromJsonAsync<GetAccessTokens.View>();

        // Assert
        Assert.NotNull(tokensContent);
        var matchedToken = tokensContent.Tokens.Single(t => t.TokenId == expectedToken.TokenId);

        Assert.Equal(expectedToken.Expires, matchedToken.Expires);
        Assert.Equal("true", matchedToken.Metadata["isTest"].ToString());
    }

    [Fact]
    public async Task API_WhenNoUserHeader_ReturnsBadRequest()
    {
        // Arrange
        var client = _appFactory.CreateClient();

        // Act
        var tokensResponse = await client.GetAsync(APIRoutes.GetUserTokens);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, tokensResponse.StatusCode);
    }

    [Fact]
    public async Task API_WhenAdmin_ReturnsAllUserTokens()
    {
        // Arrange
        var client = _appFactory.CreateClient();

        // Act
        var tokensResponse = await client.GetFromJsonAsync<GetAccessTokens.View>(APIRoutes.AdminGetTokens);

        // Assert
        Assert.Contains(tokensResponse.Tokens, t => t.UserId == "some-guy");
        Assert.Contains(tokensResponse.Tokens, t => t.UserId == "another-guy");
    }

    [Fact]
    public async Task API_WhenAdmin_AndUserFiltered_ReturnsOnlyUserTokens()
    {
        // Arrange
        var user = "some-guy";
        var client = _appFactory.CreateClient();

        // Act
        var tokensResponse = await client.GetFromJsonAsync<GetAccessTokens.View>($"{APIRoutes.AdminGetTokens}?users={user}");

        // Assert
        Assert.Contains(tokensResponse.Tokens, t => t.UserId == user);
        Assert.DoesNotContain(tokensResponse.Tokens, t => t.UserId == "another-guy");
    }

    public async Task InitializeAsync()
    {
        var client = _appFactory.CreateClient();
        await client.CreateTestToken("some-guy");
        await client.CreateTestToken("another-guy");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}