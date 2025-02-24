using System.Net.Http.Json;
using Ursa.API;

namespace Ursa.Tests;

public static class HttpClientExtensions
{
    public static async Task<TestToken> CreateTestToken(this HttpClient client, string userId)
    {
        var command = new CreateAccessToken.AdminCommand(DateTime.UtcNow.AddHours(1), userId, new Dictionary<string, object>{{ "isTest", "true" }});

        var response = await client.PostAsJsonAsync(APIRoutes.AdminCreateToken, command);
        var content = await response.Content.ReadFromJsonAsync<CreateAccessToken.View>();

        return new TestToken(content.Token, content.TokenId, content.Expires, null);
    }

    public static async Task<IEnumerable<TestToken>> GetUserTokens(this HttpClient client, string userId)
    {
        var response = await client.GetFromJsonAsync<GetAccessTokens.View>($"{APIRoutes.AdminGetTokens}?users={userId}");
        return response.Tokens.Select(t => new TestToken(t.MaskedToken, t.TokenId, t.Expires, t.Metadata));
    }

    public record TestToken(string Token, Guid TokenId, DateTimeOffset Expires, Dictionary<string, object>? Metadata);
}