using System.Text.Json.Serialization;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ursa.Tokens;

namespace Ursa.API;

public static class CreateAccessToken
{
    public record AdminCommand(DateTimeOffset Expires, string UserId, Dictionary<string, object> Metadata);

    public record Command(DateTimeOffset Expires, Dictionary<string, object> Metadata);

    public record View
    {
        [JsonConstructor]
        public View()
        {
        }

        public View(AccessToken token)
        {
            TokenId = token.TokenId;
            Token = token.RawToken;
            Expires = token.Expires;
        }

        public Guid TokenId { get; set; }
        public string? Token { get; set; }

        public DateTimeOffset Expires { get; set; }
    }

    public static Task<Results<Ok<View>, BadRequest, UnauthorizedHttpResult>> Handler(
        [FromHeader(Name = "X-User")] string userId,
        [FromBody] Command command,
        [FromServices] IDocumentStore store) 
        => CreateToken(userId, command.Expires, command.Metadata, store);

    public static Task<Results<Ok<View>, BadRequest, UnauthorizedHttpResult>> AdminHandler(
        [FromBody] AdminCommand command,
        [FromServices] IDocumentStore store) 
        => CreateToken(command.UserId, command.Expires, command.Metadata, store);

    private static async Task<Results<Ok<View>, BadRequest, UnauthorizedHttpResult>> CreateToken(
        string userId,
        DateTimeOffset expires,
        Dictionary<string, object> metadata,
        IDocumentStore store)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return TypedResults.BadRequest();
        }

        using var db = store.LightweightSession();

        var token = AccessToken.Create(
            userId,
            expires: expires,
            metadata: metadata);

        db.Store(token);
        await db.SaveChangesAsync();
        
        return TypedResults.Ok(new View(token));
    }
}