using System.Text.Json.Serialization;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ursa.Tokens;

namespace Ursa.API;

public static class GetAccessTokens
{
    public record View
    {
        [JsonConstructor]
        public View()
        {
        }

        public View(IReadOnlyList<AccessToken> tokens)
        {
            Tokens = [.. tokens.Select(t => new ViewToken(t.UserId, t.TokenId, t.MaskedToken, t.Expires, t.Metadata))];
        }

        public List<ViewToken> Tokens { get; set; } = [];

        public record ViewToken(string UserId, Guid TokenId, string MaskedToken, DateTimeOffset Expires, Dictionary<string, object> Metadata);
    }

    public static Task<Ok<View>> Handler(
        [FromHeader(Name = "X-User")] string userId,
        [FromServices] IDocumentStore store,
        [FromServices] TimeProvider time) => GetTokens([userId], store, time);

    public static Task<Ok<View>> AdminHandler(
        [FromQuery(Name = "users")] string[] userIds,
        [FromServices] IDocumentStore store,
        [FromServices] TimeProvider time) => GetTokens(userIds, store, time);

    private static async Task<Ok<View>> GetTokens(
        string[] userIds,
        IDocumentStore store,
        TimeProvider time)
    {
        var now = time.GetUtcNow();

        using var db = store.QuerySession();

        var tokens = await db.Query<AccessToken>()
            .Where(t => (userIds.Length == 0 || userIds.Contains(t.UserId)) && t.Expires > now)
            .ToListAsync();

        return TypedResults.Ok(new View(tokens));
    }
}