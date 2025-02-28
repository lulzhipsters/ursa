using System.Net;
using System.Net.Http.Json;
using JasperFx.Core;
using Ursa.API;

namespace Ursa.Tests;

public class RevokeAccessTokenTests : IClassFixture<ApplicationFixture>
{
    private const string User = "mario";

    private readonly ApplicationFactory _appFactory;

    public RevokeAccessTokenTests(ApplicationFixture fixture)
    {
        _appFactory = fixture.AppFactory!;
    }

    [Fact]
    public async Task API_User_WhenCommandIsValid_ThenTokenIsRevoked()
    {
        // Arrange
        var client = _appFactory.CreateClient().WithUserHeader(User);
        var created = await client.CreateTestToken(User);
        var command = new RevokeAccessToken.RevokeAccessTokenCommand(created.TokenId);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.RevokeToken, command);

        // Assert
        var userTokens = await client.GetUserTokens(User);

        Assert.DoesNotContain(userTokens, t => t.TokenId == created.TokenId);
    }

    [Fact]
    public async Task API_User_WhenCommandIsForOtherUser_ThenTokenIsNotRevoked()
    {
        // Arrange
        var tokenUser = "other_user";
        var client = _appFactory.CreateClient().WithUserHeader(User);
        var created = await client.CreateTestToken(tokenUser);
        var command = new RevokeAccessToken.RevokeAccessTokenCommand(created.TokenId);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.RevokeToken, command);

        // Assert
        var userTokens = await client.GetUserTokens(tokenUser);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains(userTokens, t => t.TokenId == created.TokenId);
    }

    [Fact]
    public async Task API_Admin_WhenCommandIsValid_ThenTokenIsRevoked()
    {
        // Arrange
        var tokenUser = "not_admin";
        var client = _appFactory.CreateClient();
        var created = await client.CreateTestToken(tokenUser);
        var command = new RevokeAccessToken.AdminRevokeAccessTokenCommand(created.TokenId, tokenUser);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.AdminRevokeToken, command);

        // Assert
        var userTokens = await client.GetUserTokens(User);

        Assert.DoesNotContain(userTokens, t => t.TokenId == created.TokenId);
    }
}