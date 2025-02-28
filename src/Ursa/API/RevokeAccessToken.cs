using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ursa.Tokens;

namespace Ursa.API;

public static class RevokeAccessToken
{
    public record AdminRevokeAccessTokenCommand(Guid TokenId, string UserId);

    public record RevokeAccessTokenCommand(Guid TokenId);

    public static Task<Results<Ok, StatusCodeHttpResult>> Handler(
        [FromHeader(Name = "X-User")] string userId,
        [FromBody] RevokeAccessTokenCommand command,
        [FromServices] IDocumentStore store, 
        [FromServices] TimeProvider time) => Handle(userId, command.TokenId, store, time);

    public static Task<Results<Ok, StatusCodeHttpResult>> AdminHandler(
        [FromBody] AdminRevokeAccessTokenCommand command,
        [FromServices] IDocumentStore store, 
        [FromServices] TimeProvider time) => Handle(command.UserId, command.TokenId, store, time);

    private static async Task<Results<Ok, StatusCodeHttpResult>> Handle(
        string userId,
        Guid tokenId,
        IDocumentStore store, 
        TimeProvider time)
    {
        using var db = store.LightweightSession();

        var token = await db.LoadAsync<AccessToken>(tokenId);
        
        if (token is not null)
        {
            if (token.UserId != userId)
            {
                // Not using TypedResults.Forbid here because it results in special 
                // handling to direct to an authentication scheme
                return TypedResults.StatusCode(403);
            }

            token.Expires = time.GetUtcNow();

            db.Store(token);
            await db.SaveChangesAsync();
        }

        return TypedResults.Ok();
    }
}