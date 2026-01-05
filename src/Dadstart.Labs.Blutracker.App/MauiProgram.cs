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

            var apiKey = Environment.GetEnvironmentVariable("BLUTRACKER_API_KEY");
            var options = new AzureMediaRepositoryOptions(new Uri(baseUriString), apiKey);
            return new HttpMediaRepository(httpClient, options);
        });
#endif

        builder.Services.AddSingleton<LibraryViewModel>();
        builder.Services.AddTransient<AddMovieViewModel>();
        builder.Services.AddTransient<AddTvShowViewModel>();
        builder.Services.AddTransient<AddSeasonViewModel>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<Pages.LibraryPage>();
        builder.Services.AddTransient<Pages.AddMoviePage>();
        builder.Services.AddTransient<Pages.AddTvShowPage>();
        builder.Services.AddTransient<Pages.AddSeasonPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

