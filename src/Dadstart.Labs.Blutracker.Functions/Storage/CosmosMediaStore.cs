using System.Net;
using Dadstart.Labs.Blutracker.Models;
using Microsoft.Azure.Cosmos;

namespace Dadstart.Labs.Blutracker.Functions.Storage;

public sealed class CosmosMediaStore
{
    readonly CosmosClient _client;
    readonly string _databaseName;
    readonly Lazy<Task<Container>> _movies;
    readonly Lazy<Task<Container>> _tvShows;
    readonly Lazy<Task<Container>> _seasons;

    public CosmosMediaStore(CosmosClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _databaseName = Environment.GetEnvironmentVariable("CosmosDatabaseName") ?? "Blutracker";

        _movies = new Lazy<Task<Container>>(() => GetOrCreateContainerAsync("Movies"));
        _tvShows = new Lazy<Task<Container>>(() => GetOrCreateContainerAsync("TvShows"));
        _seasons = new Lazy<Task<Container>>(() => GetOrCreateContainerAsync("Seasons"));
    }

    public async Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken)
    {
        var container = await _movies.Value.ConfigureAwait(false);
        var iterator = container.GetItemQueryIterator<MovieDocument>(new QueryDefinition("SELECT * FROM c"));

        var results = new List<Movie>();
        while (iterator.HasMoreResults)
        {
            foreach (var doc in await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false))
                results.Add(doc.ToModel());
        }

        return results;
    }

    public async Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken)
    {
        var container = await _movies.Value.ConfigureAwait(false);
        var doc = await TryReadAsync<MovieDocument>(container, id, cancellationToken).ConfigureAwait(false);
        return doc?.ToModel();
    }

    public async Task UpsertMovieAsync(Movie movie, CancellationToken cancellationToken)
    {
        var container = await _movies.Value.ConfigureAwait(false);
        var doc = MovieDocument.FromModel(movie);
        _ = await container.UpsertItemAsync(doc, new PartitionKey(doc.id), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken)
    {
        var container = await _movies.Value.ConfigureAwait(false);
        _ = await container.DeleteItemAsync<MovieDocument>(id.ToString("D"), new PartitionKey(id.ToString("D")), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TvShow>> ListTvShowsAsync(CancellationToken cancellationToken)
    {
        var container = await _tvShows.Value.ConfigureAwait(false);
        var iterator = container.GetItemQueryIterator<TvShowDocument>(new QueryDefinition("SELECT * FROM c"));

        var results = new List<TvShow>();
        while (iterator.HasMoreResults)
        {
            foreach (var doc in await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false))
                results.Add(doc.ToModel());
        }

        return results;
    }

    public async Task<TvShow?> GetTvShowAsync(Guid id, CancellationToken cancellationToken)
    {
        var container = await _tvShows.Value.ConfigureAwait(false);
        var doc = await TryReadAsync<TvShowDocument>(container, id, cancellationToken).ConfigureAwait(false);
        return doc?.ToModel();
    }

    public async Task UpsertTvShowAsync(TvShow tvShow, CancellationToken cancellationToken)
    {
        var container = await _tvShows.Value.ConfigureAwait(false);
        var doc = TvShowDocument.FromModel(tvShow);
        _ = await container.UpsertItemAsync(doc, new PartitionKey(doc.id), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteTvShowAsync(Guid id, CancellationToken cancellationToken)
    {
        // Manual cascade: seasons are stored in a separate container.
        var seasonsContainer = await _seasons.Value.ConfigureAwait(false);
        var seasonsIterator = seasonsContainer.GetItemQueryIterator<SeasonDocument>(
            new QueryDefinition("SELECT * FROM c WHERE c.tvShowId = @tvShowId")
                .WithParameter("@tvShowId", id.ToString("D")));

        while (seasonsIterator.HasMoreResults)
        {
            foreach (var season in await seasonsIterator.ReadNextAsync(cancellationToken).ConfigureAwait(false))
                _ = await seasonsContainer.DeleteItemAsync<SeasonDocument>(season.id, new PartitionKey(season.id), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        var container = await _tvShows.Value.ConfigureAwait(false);
        _ = await container.DeleteItemAsync<TvShowDocument>(id.ToString("D"), new PartitionKey(id.ToString("D")), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Season>> ListSeasonsAsync(Guid tvShowId, CancellationToken cancellationToken)
    {
        var container = await _seasons.Value.ConfigureAwait(false);
        var iterator = container.GetItemQueryIterator<SeasonDocument>(
            new QueryDefinition("SELECT * FROM c WHERE c.tvShowId = @tvShowId ORDER BY c.seasonNumber")
                .WithParameter("@tvShowId", tvShowId.ToString("D")));

        var results = new List<Season>();
        while (iterator.HasMoreResults)
        {
            foreach (var doc in await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false))
                results.Add(doc.ToModel());
        }

        return results;
    }

    public async Task<Season?> GetSeasonAsync(Guid id, CancellationToken cancellationToken)
    {
        var container = await _seasons.Value.ConfigureAwait(false);
        var doc = await TryReadAsync<SeasonDocument>(container, id, cancellationToken).ConfigureAwait(false);
        return doc?.ToModel();
    }

    public async Task UpsertSeasonAsync(Season season, CancellationToken cancellationToken)
    {
        var container = await _seasons.Value.ConfigureAwait(false);
        var doc = SeasonDocument.FromModel(season);
        _ = await container.UpsertItemAsync(doc, new PartitionKey(doc.id), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteSeasonAsync(Guid id, CancellationToken cancellationToken)
    {
        var container = await _seasons.Value.ConfigureAwait(false);
        _ = await container.DeleteItemAsync<SeasonDocument>(id.ToString("D"), new PartitionKey(id.ToString("D")), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    async Task<Container> GetOrCreateContainerAsync(string containerName)
    {
        var database = await _client.CreateDatabaseIfNotExistsAsync(_databaseName).ConfigureAwait(false);
        var container = await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id").ConfigureAwait(false);
        return container.Container;
    }

    static async Task<T?> TryReadAsync<T>(Container container, Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await container.ReadItemAsync<T>(id.ToString("D"), new PartitionKey(id.ToString("D")), cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    sealed record MovieDocument(string id, string title, int? releaseYear, string createdAt)
    {
        public static MovieDocument FromModel(Movie movie)
            => new(movie.Id.ToString("D"), movie.Title, movie.ReleaseYear, movie.CreatedAt.ToString("O"));

        public Movie ToModel()
            => new(Guid.Parse(id), title, releaseYear, DateTimeOffset.Parse(createdAt));
    }

    sealed record TvShowDocument(string id, string title, string createdAt)
    {
        public static TvShowDocument FromModel(TvShow show)
            => new(show.Id.ToString("D"), show.Title, show.CreatedAt.ToString("O"));

        public TvShow ToModel()
            => new(Guid.Parse(id), title, DateTimeOffset.Parse(createdAt));
    }

    sealed record SeasonDocument(string id, string tvShowId, int seasonNumber, int? episodeCount, string createdAt)
    {
        public static SeasonDocument FromModel(Season season)
            => new(season.Id.ToString("D"), season.TvShowId.ToString("D"), season.SeasonNumber, season.EpisodeCount, season.CreatedAt.ToString("O"));

        public Season ToModel()
            => new(Guid.Parse(id), Guid.Parse(tvShowId), seasonNumber, episodeCount, DateTimeOffset.Parse(createdAt));
    }
}

