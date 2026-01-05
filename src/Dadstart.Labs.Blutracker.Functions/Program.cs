using Dadstart.Labs.Blutracker.Functions.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set CosmosConnectionString environment variable.");

        services.AddSingleton(_ => new CosmosClient(connectionString));
        services.AddSingleton<CosmosMediaStore>();
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
