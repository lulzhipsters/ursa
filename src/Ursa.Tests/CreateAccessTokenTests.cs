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
    public async Task API_User_WhenCommandIsValid_ThenTokenIsCreated()
    {
        // Arrange
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var metadata = new Dictionary<string, object> { { "key", "value" } };
        var command = new CreateAccessToken.Command(expires, metadata);

        var client = _appFactory.CreateClient().WithUserHeader(User);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.CreateToken, command);
        var created = await response.Content.ReadFromJsonAsync<CreateAccessToken.View>();

        // Assert
        Assert.NotNull(created);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(Guid.Empty, created.TokenId);
        Assert.NotNull(created.Token);
        Assert.NotEmpty(created.Token);
        Assert.Equal(expires, created.Expires);

        var retrieved = (await client.GetUserTokens(User))
            .Single(t => t.TokenId == created.TokenId);

        Assert.Equal(expires, retrieved.Expires);
        Assert.Equal("value", retrieved.Metadata["key"].ToString());
    }

   [Fact]
    public async Task API_User_WhenExpiryIsInPast_ThenReturnBadRequest()
    {
        // Arrange
        var metadataId = Guid.NewGuid().ToString(); // use this Id to assert token wasn't created from *this* run
        var expires = DateTimeOffset.UtcNow.AddHours(-1); // Invalid: expiration in the past
        var command = new CreateAccessToken.Command(expires, new() { { "id", metadataId } });

        var client = _appFactory.CreateClient().WithUserHeader(User);

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.CreateToken, command);

        // Assert
        await AssertNotCreated(metadataId, client, response);
    }

    [Fact]
    public async Task API_User_WhenNoUserHeader_ThenReturnBadRequest()
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

    [Fact]
    public async Task API_Admin_WhenCommandIsValid_ThenTokenIsCreated()
    {
        // Arrange
        var tokenUser = "not_admin";
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var metadata = new Dictionary<string, object> { { "key", "value" } };
        var command = new CreateAccessToken.AdminCommand(expires, tokenUser, metadata);

        var client = _appFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.AdminCreateToken, command);
        var created = await response.Content.ReadFromJsonAsync<CreateAccessToken.View>();

        // Assert
        Assert.NotNull(created);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(Guid.Empty, created.TokenId);
        Assert.NotNull(created.Token);
        Assert.NotEmpty(created.Token);
        Assert.Equal(expires, created.Expires);

        var retrieved = (await client.GetUserTokens(tokenUser))
            .Single(t => t.TokenId == created.TokenId);

        Assert.Equal(expires, retrieved.Expires);
        Assert.Equal("value", retrieved.Metadata["key"].ToString());
    }

   [Fact]
    public async Task API_Admin_WhenExpiryIsInPast_ThenReturnBadRequest()
    {
        // Arrange
        var tokenUser = "not_admin";
        var metadataId = Guid.NewGuid().ToString(); // use this Id to assert token wasn't created from *this* run
        var expires = DateTimeOffset.UtcNow.AddHours(-1); // Invalid: expiration in the past
        var command = new CreateAccessToken.AdminCommand(expires, tokenUser, new() { { "id", metadataId } });

        var client = _appFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.AdminCreateToken, command);

        // Assert
        await AssertNotCreated(metadataId, client, response);
    }

   [Fact]
    public async Task API_Admin_WhenUserIdIsEmpty_ThenReturnBadRequest()
    {
        // Arrange
        var metadataId = Guid.NewGuid().ToString(); // use this Id to assert token wasn't created from *this* run
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var command = new CreateAccessToken.AdminCommand(expires, string.Empty, new() { { "id", metadataId } });

        var client = _appFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(APIRoutes.AdminCreateToken, command);

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
}