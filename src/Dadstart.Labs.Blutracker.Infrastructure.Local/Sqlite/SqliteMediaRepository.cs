using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;
using Microsoft.Data.Sqlite;

namespace Dadstart.Labs.Blutracker.Infrastructure.Local.Sqlite;

public sealed class SqliteMediaRepository : IMediaRepository
{
    readonly string _connectionString;

    public SqliteMediaRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException("Database path is required.", nameof(databasePath));

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true
        }.ToString();
    }

    public async Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             SELECT Id, Title, ReleaseYear, CreatedAt
                             FROM Movies
                             ORDER BY Title COLLATE NOCASE;
                             """;

        var results = new List<Movie>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = Guid.Parse(reader.GetString(0));
            var title = reader.GetString(1);
            int? year = reader.IsDBNull(2) ? null : reader.GetInt32(2);
            var createdAt = DateTimeOffset.Parse(reader.GetString(3));

            results.Add(new Movie(id, title, year, createdAt));
        }

        return results;
    }

    public async Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             SELECT Id, Title, ReleaseYear, CreatedAt
                             FROM Movies
                             WHERE Id = $id;
                             """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var title = reader.GetString(1);
        int? year = reader.IsDBNull(2) ? null : reader.GetInt32(2);
        var createdAt = DateTimeOffset.Parse(reader.GetString(3));

        return new Movie(id, title, year, createdAt);
    }

    public async Task UpsertMovieAsync(Movie movie, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             INSERT INTO Movies(Id, Title, ReleaseYear, CreatedAt)
                             VALUES($id, $title, $releaseYear, $createdAt)
                             ON CONFLICT(Id) DO UPDATE SET
                                 Title = excluded.Title,
                                 ReleaseYear = excluded.ReleaseYear;
                             """;
        command.Parameters.AddWithValue("$id", movie.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", movie.Title);
        command.Parameters.AddWithValue("$releaseYear", (object?)movie.ReleaseYear ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", movie.CreatedAt.ToString("O"));

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Movies WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TvShow>> ListTvShowsAsync(CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             SELECT Id, Title, CreatedAt
                             FROM TvShows
                             ORDER BY Title COLLATE NOCASE;
                             """;

        var results = new List<TvShow>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = Guid.Parse(reader.GetString(0));
            var title = reader.GetString(1);
            var createdAt = DateTimeOffset.Parse(reader.GetString(2));

            results.Add(new TvShow(id, title, createdAt));
        }

        return results;
    }

    public async Task<TvShow?> GetTvShowAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             SELECT Id, Title, CreatedAt
                             FROM TvShows
                             WHERE Id = $id;
                             """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var title = reader.GetString(1);
        var createdAt = DateTimeOffset.Parse(reader.GetString(2));

        return new TvShow(id, title, createdAt);
    }

    public async Task UpsertTvShowAsync(TvShow tvShow, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             INSERT INTO TvShows(Id, Title, CreatedAt)
                             VALUES($id, $title, $createdAt)
                             ON CONFLICT(Id) DO UPDATE SET
                                 Title = excluded.Title;
                             """;
        command.Parameters.AddWithValue("$id", tvShow.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", tvShow.Title);
        command.Parameters.AddWithValue("$createdAt", tvShow.CreatedAt.ToString("O"));

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteTvShowAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM TvShows WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Season>> ListSeasonsAsync(Guid tvShowId, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             SELECT Id, TvShowId, SeasonNumber, EpisodeCount, CreatedAt
                             FROM Seasons
                             WHERE TvShowId = $tvShowId
                             ORDER BY SeasonNumber;
                             """;
        command.Parameters.AddWithValue("$tvShowId", tvShowId.ToString("D"));

        var results = new List<Season>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = Guid.Parse(reader.GetString(0));
            var showId = Guid.Parse(reader.GetString(1));
            var seasonNumber = reader.GetInt32(2);
            int? episodeCount = reader.IsDBNull(3) ? null : reader.GetInt32(3);
            var createdAt = DateTimeOffset.Parse(reader.GetString(4));

            results.Add(new Season(id, showId, seasonNumber, episodeCount, createdAt));
        }

        return results;
    }

    public async Task UpsertSeasonAsync(Season season, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             INSERT INTO Seasons(Id, TvShowId, SeasonNumber, EpisodeCount, CreatedAt)
                             VALUES($id, $tvShowId, $seasonNumber, $episodeCount, $createdAt)
                             ON CONFLICT(Id) DO UPDATE SET
                                 TvShowId = excluded.TvShowId,
                                 SeasonNumber = excluded.SeasonNumber,
                                 EpisodeCount = excluded.EpisodeCount;
                             """;
        command.Parameters.AddWithValue("$id", season.Id.ToString("D"));
        command.Parameters.AddWithValue("$tvShowId", season.TvShowId.ToString("D"));
        command.Parameters.AddWithValue("$seasonNumber", season.SeasonNumber);
        command.Parameters.AddWithValue("$episodeCount", (object?)season.EpisodeCount ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", season.CreatedAt.ToString("O"));

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteSeasonAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Seasons WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                             CREATE TABLE IF NOT EXISTS Movies(
                                 Id TEXT PRIMARY KEY NOT NULL,
                                 Title TEXT NOT NULL,
                                 ReleaseYear INTEGER NULL,
                                 CreatedAt TEXT NOT NULL
                             );

                             CREATE TABLE IF NOT EXISTS TvShows(
                                 Id TEXT PRIMARY KEY NOT NULL,
                                 Title TEXT NOT NULL,
                                 CreatedAt TEXT NOT NULL
                             );

                             CREATE TABLE IF NOT EXISTS Seasons(
                                 Id TEXT PRIMARY KEY NOT NULL,
                                 TvShowId TEXT NOT NULL,
                                 SeasonNumber INTEGER NOT NULL,
                                 EpisodeCount INTEGER NULL,
                                 CreatedAt TEXT NOT NULL,
                                 FOREIGN KEY(TvShowId) REFERENCES TvShows(Id) ON DELETE CASCADE
                             );

                             CREATE INDEX IF NOT EXISTS IX_Seasons_TvShowId ON Seasons(TvShowId);
                             """;

        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}

