using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ursa.Tests;

public class ApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string> _settings = [];

    public ApplicationFactory(Dictionary<string, string> configuration)
    {
        Dictionary<string, string> defaults = new()
        {
        };

        _settings = configuration
            .Union(defaults)
            .ToDictionary();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(_settings!)
            .Build());

        builder.UseEnvironment("Development"); // required to have db schema automatically applied on startup
    }
}