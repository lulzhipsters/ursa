using System.Net;
using System.Net.Http.Json;
using JasperFx.Core;
using Testcontainers.PostgreSql;
using Ursa.API;

namespace Ursa.Tests;

public class GetUserInfoTests : IClassFixture<ApplicationFixture>
{
    private const string User = "mario";

    private readonly ApplicationFactory _appFactory;

    public GetUserInfoTests(ApplicationFixture fixture)
    {
        _appFactory = fixture.AppFactory!;
    }

    [Fact]
    public async Task API_User_WhenHeaderIsValid_ThenUserInfoReturned()
    {
        // Arrange
        var client = _appFactory.CreateClient();
        var created = await client.CreateTestToken(User);

        client.WithAuthHeader(created.Token);

        // Act
        var response = await client.GetFromJsonAsync<GetUserInfo.View>(APIRoutes.GetUser);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(User, response.UserId);
        Assert.Equal("true", response.Metadata["isTest"].ToString());
    }

    [Fact]
    public async Task API_User_WhenHeaderIsInvalid_ThenReturnsUnauthorized()
    {
        // Arrange
        var client = _appFactory
            .CreateClient()
            .WithAuthHeader("notarealtoken");

        // Act
        var response = await client.GetAsync(APIRoutes.GetUser);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}