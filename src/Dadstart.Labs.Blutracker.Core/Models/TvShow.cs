namespace Dadstart.Labs.Blutracker.Models;

public sealed record TvShow(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt);

