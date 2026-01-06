using Dadstart.Labs.Blutracker.Infrastructure.Local.Sqlite;
using Dadstart.Labs.Blutracker.Models;
using Xunit;

namespace Dadstart.Labs.Blutracker.Tests;

public sealed class SqliteMediaRepositoryTests
{
    [Fact]
    public async Task Upsert_and_list_movies_roundtrips()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            var repository = new SqliteMediaRepository(dbPath);
            var movie = new Movie(Guid.NewGuid(), "Blade Runner", 1982, DateTimeOffset.UtcNow);

            await repository.UpsertMovieAsync(movie, CancellationToken.None);

            var movies = await repository.ListMoviesAsync(CancellationToken.None);

            Assert.Single(movies);
            Assert.Equal(movie.Id, movies[0].Id);
            Assert.Equal(movie.Title, movies[0].Title);
            Assert.Equal(movie.ReleaseYear, movies[0].ReleaseYear);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task Seasons_are_cascaded_when_tvshow_deleted()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            var repository = new SqliteMediaRepository(dbPath);
            var show = new TvShow(Guid.NewGuid(), "The Expanse", DateTimeOffset.UtcNow);
            var season = new Season(Guid.NewGuid(), show.Id, 1, 10, DateTimeOffset.UtcNow);

            await repository.UpsertTvShowAsync(show, CancellationToken.None);
            await repository.UpsertSeasonAsync(season, CancellationToken.None);

            var seasonsBefore = await repository.ListSeasonsAsync(show.Id, CancellationToken.None);
            Assert.Single(seasonsBefore);

            var fetched = await repository.GetSeasonAsync(season.Id, CancellationToken.None);
            Assert.NotNull(fetched);
            Assert.Equal(season.Id, fetched!.Id);
            Assert.Equal(show.Id, fetched.TvShowId);

            await repository.DeleteTvShowAsync(show.Id, CancellationToken.None);

            var seasonsAfter = await repository.ListSeasonsAsync(show.Id, CancellationToken.None);
            Assert.Empty(seasonsAfter);

            var fetchedAfterDelete = await repository.GetSeasonAsync(season.Id, CancellationToken.None);
            Assert.Null(fetchedAfterDelete);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

