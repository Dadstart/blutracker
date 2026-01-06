namespace Dadstart.Labs.Blutracker.Models;

public sealed record Season(
    Guid Id,
    Guid TvShowId,
    int SeasonNumber,
    int? EpisodeCount,
    DateTimeOffset CreatedAt);

