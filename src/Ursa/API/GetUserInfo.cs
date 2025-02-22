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
        private View(AccessToken? token = null)
        {
            UserId = token?.UserId;
            Metadata = token?.Metadata ?? [];
        }

        public string? UserId { get; private set; }

        public Dictionary<string, object> Metadata { get; set;} = [];

        [JsonIgnore]
        public bool Found => UserId is not null;

        public static View NotFound() => new();
        public static View Create(AccessToken token) => new(token);
    }

    public static async Task<Results<Ok<View>, NotFound>> Handler(
        [FromHeader] string authorization,
        [FromServices] IDocumentStore store)
    {
        ArgumentException.ThrowIfNullOrEmpty(authorization);

        var bearerToken = authorization.Replace("Bearer ", "");

        using var db = store.QuerySession();

        var hashedToken = CryptoHelper.Hash(bearerToken, CryptoHelper.DefaultSalt);
        var token = await db.Query<AccessToken>().FirstOrDefaultAsync(t => t.HashedToken == hashedToken);

        return token is null 
            ? TypedResults.NotFound()
            : TypedResults.Ok(View.Create(token));
    }
}