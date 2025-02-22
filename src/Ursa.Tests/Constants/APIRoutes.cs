namespace Ursa.Tests;

public static class APIRoutes
{
    public const string AdminCreateToken = "/admin/commands/create-token/";
    public const string AdminRevokeToken = "/admin/commands/revoke-token/";
    public const string AdminGetTokens = "/admin/tokens";

    public const string CreateToken = "/commands/create-token/";
    public const string RevokeToken = "/commands/revoke-token/";
    public const string GetUser = "/users/current/";
    public const string GetUserTokens = "/users/current/tokens/";
}