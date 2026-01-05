namespace Dadstart.Labs.Blutracker.Infrastructure.Azure;

public sealed record AzureMediaRepositoryOptions(
    Uri BaseUri,
    string? ApiKey);

