using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Ursa.API;

namespace Ursa.Tests;

public class CreateAccessTokenTests : IClassFixture<ApplicationFixture>
{
    private readonly ApplicationFactory _appFactory;
    private readonly PostgreSqlContainer _database;

    public CreateAccessTokenTests(ApplicationFixture fixture)
    {
        _appFactory = fixture.AppFactory!;
        _database = fixture.Database;
    }

    [Fact]
    public async Task Handler_WhenCommandIsValid_ThenTokenIsCreated()
    {
        // Arrange
        var userId = "user123";
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var metadata = new Dictionary<string, object> { { "key", "value" } };
        var command = new CreateAccessToken.Command(expires, metadata);

        var client = _appFactory.CreateClient();
        client.DefaultRequestHeaders.Add(Headers.XUser, userId);

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


    // [Theory]
    // [InlineData("")]
    // [InlineData(null)]
    // public async Task Handler_WhenUserIdIsEmpty_ThenReturnsBadRequest(string? userId)
    // {
    //     // Arrange
    //     var command = new CreateAccessToken.Command(DateTimeOffset.UtcNow.AddHours(1), []);

    //     // Act
    //     var result = await CreateAccessToken.Handler(userId, command, _store);

    //     // Assert
    //     Assert.IsType<BadRequest>(result.Result);

    //     await _session.DidNotReceive()
    //         .SaveChangesAsync(Arg.Any<CancellationToken>());

    //     _session.DidNotReceive()
    //         .Store(Arg.Any<AccessToken>());
    // }

    // [Fact]
    // public async Task AdminHandler_WhenCommandIsValid_ThenTokenIsCreated()
    // {
    //     // Arrange
    //     var userId = "user123";
    //     var expires = DateTimeOffset.UtcNow.AddHours(1);
    //     var metadata = new Dictionary<string, object> { { "key", "value" } };
    //     var command = new CreateAccessToken.AdminCommand(expires, userId, metadata);

    //     // Act
    //     var result = await CreateAccessToken.AdminHandler(command, _store);

    //     // Assert
    //     var okResult = Assert.IsType<Ok<CreateAccessToken.View>>(result.Result);
    //     Assert.NotEqual(Guid.Empty, okResult.Value!.TokenId);
    //     Assert.NotNull(okResult.Value.Token);
    //     Assert.NotEmpty(okResult.Value.Token);
    //     Assert.Equal(expires, okResult.Value.Expires);

    //     await _session.Received(1)
    //         .SaveChangesAsync(Arg.Any<CancellationToken>());

    //     _session.Received(1)
    //         .Store(Arg.Is<AccessToken>(t => 
    //             t.UserId == userId && 
    //             t.Expires == expires));
    // }

    // [Theory]
    // [InlineData("")]
    // [InlineData(null)]
    // public async Task AdminHandler_WhenUserIdIsEmpty_ThenReturnsBadRequest(string? userId)
    // {
    //     // Arrange
    //     var command = new CreateAccessToken.AdminCommand(DateTimeOffset.UtcNow.AddHours(1), userId, []);

    //     // Act
    //     var result = await CreateAccessToken.AdminHandler(command, _store);

    //     // Assert
    //     Assert.IsType<BadRequest>(result.Result);
        
    //     await _session.DidNotReceive()
    //         .SaveChangesAsync(Arg.Any<CancellationToken>());

    //     _session.DidNotReceive()
    //         .Store(Arg.Any<AccessToken>());
    // }

    // [Fact]
    // public void View_Constructor_MapsPropertiesCorrectly()
    // {
    //     // Arrange
    //     var tokenId = Guid.NewGuid();
    //     var rawToken = "test-token";
    //     var expires = DateTimeOffset.UtcNow.AddHours(1);
    //     var token = new AccessToken
    //     {
    //         TokenId = tokenId,
    //         UserId = "user123",
    //         RawToken = rawToken,
    //         Expires = expires,
    //     };

    //     // Act
    //     var view = new CreateAccessToken.View(token);

    //     // Assert
    //     Assert.Equal(tokenId, view.TokenId);
    //     Assert.Equal(rawToken, view.Token);
    //     Assert.Equal(expires, view.Expires);
    // }
}