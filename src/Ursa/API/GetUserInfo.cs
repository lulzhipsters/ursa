using System.Text.Json.Serialization;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ursa.Tokens;

namespace Ursa.API;

public static class GetUserInfo
{
    public record View
    {
        [JsonConstructor]
        public View()
        {
        }

        public View(AccessToken? token = null)
        {
            UserId = token?.UserId;
            Metadata = token?.Metadata ?? [];
        }

        public string? UserId { get; set; }

        public Dictionary<string, object> Metadata { get; set;} = [];

        public static View Create(AccessToken token) => new(token);
    }

    public static async Task<Results<Ok<View>, UnauthorizedHttpResult>> Handler(
        [FromHeader] string authorization,
        [FromServices] IDocumentStore store)
    {
        ArgumentException.ThrowIfNullOrEmpty(authorization);

        var bearerToken = authorization.Replace("Bearer ", "");

        using var db = store.QuerySession();

        var hashedToken = CryptoHelper.Hash(bearerToken, CryptoHelper.DefaultSalt);
        var token = await db.Query<AccessToken>().FirstOrDefaultAsync(t => t.HashedToken == hashedToken);

        return token is null 
            ? TypedResults.Unauthorized()
            : TypedResults.Ok(new View(token));
    }
}