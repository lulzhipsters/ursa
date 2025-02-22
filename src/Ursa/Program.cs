using dotenv.net;
using Marten;
using Oakton;
using Ursa;
using Ursa.API;
using Ursa.Tokens;

DotEnv.Load(new DotEnvOptions(
    ignoreExceptions: true,
    probeForEnv: true));
    
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var settings = builder.Configuration.Get<Settings>()!;

builder.Services.AddMarten(options => {
    options.DisableNpgsqlLogging = true; // Overly verbose (probably) logs

    // connection string in npgsql format https://www.npgsql.org/doc/connection-string-parameters.html
    options.Connection(settings.DatabaseConnection);

    options.UseSystemTextJsonForSerialization(casing: Casing.CamelCase);

    options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.CreateOrUpdate;

    options.RegisterDocumentType<AccessToken>();
})
.ApplyAllDatabaseChangesOnStartup();

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.MapPost("/admin/commands/revoke-token", RevokeAccessToken.AdminHandler);
app.MapPost("/admin/commands/create-token", CreateAccessToken.AdminHandler);
app.MapGet("/admin/tokens", GetAccessTokens.AdminHandler);

app.MapPost("/commands/revoke-token", RevokeAccessToken.Handler);
app.MapPost("/commands/create-token", CreateAccessToken.Handler);
app.MapGet("/users/current", GetUserInfo.Handler);
app.MapGet("/users/current/tokens", GetAccessTokens.Handler);

app.Run();

// Just here to make this class public for test project
public partial class Program {}