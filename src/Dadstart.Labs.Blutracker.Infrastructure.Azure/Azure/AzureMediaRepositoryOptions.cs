namespace Dadstart.Labs.Blutracker.Infrastructure.Azure;

public enum AzureApiAuthMode
{
    None = 0,
    FunctionKey = 1,
    BearerToken = 2
}

public sealed record AzureMediaRepositoryOptions(
    Uri BaseUri,
    AzureApiAuthMode AuthMode,
    string? FunctionKey,
    string? BearerToken)
{
    public static AzureMediaRepositoryOptions FunctionKeyAuth(Uri baseUri, string functionKey)
        => new(baseUri, AzureApiAuthMode.FunctionKey, functionKey, null);

    public static AzureMediaRepositoryOptions BearerTokenAuth(Uri baseUri, string bearerToken)
        => new(baseUri, AzureApiAuthMode.BearerToken, null, bearerToken);

    public static AzureMediaRepositoryOptions Anonymous(Uri baseUri)
        => new(baseUri, AzureApiAuthMode.None, null, null);
}

