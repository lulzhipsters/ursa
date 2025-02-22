using Testcontainers.PostgreSql;

namespace Ursa.Tests;

public class ApplicationFixture : IAsyncLifetime
{
    const string DBName = "ursa";
    const string DBPassword = "test";
    const string DBUser = "test";

    public PostgreSqlContainer Database;
    public ApplicationFactory? AppFactory { get; private set; }


    public ApplicationFixture()
    {
        Database = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithEnvironment("POSTGRES_DB", DBName)
            .WithEnvironment("POSTGRES_PASSWORD", DBPassword)
            .WithEnvironment("POSTGRES_USER", DBUser)
            .Build();
    }

    public async Task InitializeAsync()
    {       
        await Database.StartAsync();

        AppFactory = new ApplicationFactory(new (){
            {"URSA_DATABASE_CONNECTION", $"Host=localhost; Port={Database.GetMappedPublicPort(5432)}; Database={DBName}; Username={DBUser}; Password={DBPassword}; Persist Security Info=true;"}
        });
    }

    Task IAsyncLifetime.DisposeAsync() => Task.WhenAll([
        Database.DisposeAsync().AsTask(),
        AppFactory?.DisposeAsync().AsTask() ?? Task.CompletedTask
    ]);
}
