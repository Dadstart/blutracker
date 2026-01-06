using Dadstart.Labs.Blutracker.Models;

namespace Dadstart.Labs.Blutracker.Storage;

public interface IMediaRepository
{
    Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken);
    Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken);
    Task UpsertMovieAsync(Movie movie, CancellationToken cancellationToken);
    Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TvShow>> ListTvShowsAsync(CancellationToken cancellationToken);
    Task<TvShow?> GetTvShowAsync(Guid id, CancellationToken cancellationToken);
    Task UpsertTvShowAsync(TvShow tvShow, CancellationToken cancellationToken);
    Task DeleteTvShowAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Season>> ListSeasonsAsync(Guid tvShowId, CancellationToken cancellationToken);
    Task<Season?> GetSeasonAsync(Guid id, CancellationToken cancellationToken);
    Task UpsertSeasonAsync(Season season, CancellationToken cancellationToken);
    Task DeleteSeasonAsync(Guid id, CancellationToken cancellationToken);
}

