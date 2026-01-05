using Dadstart.Labs.Blutracker.Infrastructure.Azure;
using Dadstart.Labs.Blutracker.Infrastructure.Local.Sqlite;
using Dadstart.Labs.Blutracker.Storage;
using Dadstart.Labs.Blutracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dadstart.Labs.Blutracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(_ => { });

#if DEBUG
        builder.Services.AddSingleton<IMediaRepository>(_ =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "blutracker.db");
            return new SqliteMediaRepository(dbPath);
        });
#else
        builder.Services.AddHttpClient<HttpMediaRepository>();
        builder.Services.AddSingleton<IMediaRepository>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var baseUriString = Environment.GetEnvironmentVariable("BLUTRACKER_API_BASEURI");

            if (string.IsNullOrWhiteSpace(baseUriString))
                throw new InvalidOperationException("Set BLUTRACKER_API_BASEURI (e.g. https://your-function.azurewebsites.net/).");

            var baseUri = new Uri(baseUriString);
            var authMode = (Environment.GetEnvironmentVariable("BLUTRACKER_AUTH_MODE") ?? "FunctionKey").Trim();

            AzureMediaRepositoryOptions options;

            if (authMode.Equals("Bearer", StringComparison.OrdinalIgnoreCase) ||
                authMode.Equals("BearerToken", StringComparison.OrdinalIgnoreCase))
            {
                var token = Environment.GetEnvironmentVariable("BLUTRACKER_BEARER_TOKEN");
                options = AzureMediaRepositoryOptions.BearerTokenAuth(baseUri, token ?? string.Empty);
            }
            else if (authMode.Equals("None", StringComparison.OrdinalIgnoreCase) ||
                     authMode.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
            {
                options = AzureMediaRepositoryOptions.Anonymous(baseUri);
            }
            else
            {
                var functionKey = Environment.GetEnvironmentVariable("BLUTRACKER_FUNCTION_KEY") ??
                                  Environment.GetEnvironmentVariable("BLUTRACKER_API_KEY");
                options = AzureMediaRepositoryOptions.FunctionKeyAuth(baseUri, functionKey ?? string.Empty);
            }

            return new HttpMediaRepository(httpClient, options);
        });
#endif

        builder.Services.AddSingleton<LibraryViewModel>();
        builder.Services.AddTransient<AddMovieViewModel>();
        builder.Services.AddTransient<AddTvShowViewModel>();
        builder.Services.AddTransient<AddSeasonViewModel>();
        builder.Services.AddTransient<TvShowDetailViewModel>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<Pages.LibraryPage>();
        builder.Services.AddTransient<Pages.AddMoviePage>();
        builder.Services.AddTransient<Pages.AddTvShowPage>();
        builder.Services.AddTransient<Pages.AddSeasonPage>();
        builder.Services.AddTransient<Pages.TvShowDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

