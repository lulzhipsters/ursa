using System.Net.Http.Json;
using JasperFx.Core;
using Testcontainers.PostgreSql;
using Ursa.API;

namespace Ursa.Tests;

public class CreateAccessTokenTests : IClassFixture<ApplicationFixture>
{
    private const string User = "mario";

    private readonly ApplicationFactory _appFactory;

    public CreateAccessTokenTests(ApplicationFixture fixture)
    {
        _appFactory = fixture.AppFactory!;
    }

    [Fact]
    public async Task API_WhenCommandIsValid_ThenTokenIsCreated()
    {
        // Arrange
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var metadata = new Dictionary<string, object> { { "key", "value" } };
        var command = new CreateAccessToken.Command(expires, metadata);

        var client = _appFactory.CreateClient();
        client.DefaultRequestHeaders.Add(Headers.XUser, User);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.CreateToken, command);
        var created = await response.Content.ReadFromJsonAsync<CreateAccessToken.View>();

        var userTokens = await client.GetFromJsonAsync<GetAccessTokens.View>(APIRoutes.GetUserTokens);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(Guid.Empty, created.TokenId);
        Assert.NotNull(created.Token);
        Assert.NotEmpty(created.Token);
        Assert.Equal(expires, created.Expires);

        Assert.NotNull(userTokens);

        var token =  userTokens.Tokens.Single(t => t.TokenId == created.TokenId);

        Assert.Equal(expires, token.Expires);
        Assert.Equal("value", token.Metadata["key"].ToString());
    }

   [Fact]
    public async Task API_WhenExpiryIsInPast_ThenReturnBadRequest()
    {
        // Arrange
        var metadataId = Guid.NewGuid().ToString(); // use this Id to assert token wasn't created from *this* run
        var expires = DateTimeOffset.UtcNow.AddHours(-1); // Invalid: expiration in the past
        var command = new CreateAccessToken.Command(expires, new() { { "id", metadataId } });

        var client = _appFactory.CreateClient();
        client.DefaultRequestHeaders.Add(Headers.XUser, User);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.CreateToken, command);

        // Assert
        await AssertNotCreated(metadataId, client, response);
    }

    [Fact]
    public async Task API_WhenNoUserHeader_ThenReturnBadRequest()
    {
        // Arrange
        var metadataId = Guid.NewGuid().ToString(); // use this Id to assert token wasn't created from *this* run
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var command = new CreateAccessToken.Command(expires, new() { { "id", metadataId } });

        var client = _appFactory.CreateClient(); // Intentionally not adding the X-User header
        
        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.CreateToken, command);

        // Assert
        await AssertNotCreated(metadataId, client, response);
    }

    private static async Task AssertNotCreated(string metadataId, HttpClient client, HttpResponseMessage response)
    {
        var userTokens = await client.GetUserTokens(User);
        var token = userTokens.FirstOrDefault(t => t.Metadata!.GetValueOrDefault("id")?.ToString() == metadataId);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(userTokens);
        Assert.Null(token);
    }

    private record TestToken(string Token, Guid TokenId, DateTimeOffset Expires);
}