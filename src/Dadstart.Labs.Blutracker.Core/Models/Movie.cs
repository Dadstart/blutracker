namespace Dadstart.Labs.Blutracker.Models;

public sealed record Movie(
    Guid Id,
    string Title,
    int? ReleaseYear,
    DateTimeOffset CreatedAt);

