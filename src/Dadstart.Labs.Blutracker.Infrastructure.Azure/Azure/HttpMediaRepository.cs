using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.Infrastructure.Azure;

public sealed class HttpMediaRepository : IMediaRepository
{
    readonly HttpClient _httpClient;

    public HttpMediaRepository(HttpClient httpClient, AzureMediaRepositoryOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (options.BaseUri is null)
            throw new ArgumentException("BaseUri is required.", nameof(options));

        _httpClient.BaseAddress = options.BaseUri;

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            _httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken)
        => await GetAsync<IReadOnlyList<Movie>>("api/movies", cancellationToken).ConfigureAwait(false) ?? Array.Empty<Movie>();

    public async Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken)
        => await GetAsync<Movie>($"api/movies/{id:D}", cancellationToken).ConfigureAwait(false);

    public async Task UpsertMovieAsync(Movie movie, CancellationToken cancellationToken)
        => await PutAsync($"api/movies/{movie.Id:D}", movie, cancellationToken).ConfigureAwait(false);

    public async Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken)
        => await DeleteAsync($"api/movies/{id:D}", cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<TvShow>> ListTvShowsAsync(CancellationToken cancellationToken)
        => await GetAsync<IReadOnlyList<TvShow>>("api/tvshows", cancellationToken).ConfigureAwait(false) ?? Array.Empty<TvShow>();

    public async Task<TvShow?> GetTvShowAsync(Guid id, CancellationToken cancellationToken)
        => await GetAsync<TvShow>($"api/tvshows/{id:D}", cancellationToken).ConfigureAwait(false);

    public async Task UpsertTvShowAsync(TvShow tvShow, CancellationToken cancellationToken)
        => await PutAsync($"api/tvshows/{tvShow.Id:D}", tvShow, cancellationToken).ConfigureAwait(false);

    public async Task DeleteTvShowAsync(Guid id, CancellationToken cancellationToken)
        => await DeleteAsync($"api/tvshows/{id:D}", cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Season>> ListSeasonsAsync(Guid tvShowId, CancellationToken cancellationToken)
        => await GetAsync<IReadOnlyList<Season>>($"api/tvshows/{tvShowId:D}/seasons", cancellationToken).ConfigureAwait(false) ?? Array.Empty<Season>();

    public async Task UpsertSeasonAsync(Season season, CancellationToken cancellationToken)
        => await PutAsync($"api/tvshows/{season.TvShowId:D}/seasons/{season.Id:D}", season, cancellationToken).ConfigureAwait(false);

    public async Task DeleteSeasonAsync(Guid id, CancellationToken cancellationToken)
        => await DeleteAsync($"api/seasons/{id:D}", cancellationToken).ConfigureAwait(false);

    async Task<T?> GetAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(relativeUri, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    async Task PutAsync<T>(string relativeUri, T body, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PutAsJsonAsync(relativeUri, body, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    async Task DeleteAsync(string relativeUri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync(relativeUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}

