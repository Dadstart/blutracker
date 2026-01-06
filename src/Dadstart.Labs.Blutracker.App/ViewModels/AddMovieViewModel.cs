using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class AddMovieViewModel : ObservableObject
{
    readonly IMediaRepository _repository;
    Guid? _movieId;
    DateTimeOffset _createdAt;

    [ObservableProperty]
    string _title = string.Empty;

    [ObservableProperty]
    string _releaseYear = string.Empty;

    [ObservableProperty]
    bool _isBusy;

    public AddMovieViewModel(IMediaRepository repository) => _repository = repository;

    public bool IsEditMode => _movieId is not null;

    public string PageTitle => IsEditMode ? "Edit movie" : "Add movie";

    public async Task LoadAsync(Guid id)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var movie = await _repository.GetMovieAsync(id, CancellationToken.None).ConfigureAwait(false);
            if (movie is null)
                return;

            _movieId = movie.Id;
            _createdAt = movie.CreatedAt;
            Title = movie.Title;
            ReleaseYear = movie.ReleaseYear?.ToString() ?? string.Empty;

            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(PageTitle));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Title))
            return;

        try
        {
            IsBusy = true;

            int? year = int.TryParse(ReleaseYear, out var parsed) ? parsed : null;
            var movieId = _movieId ?? Guid.NewGuid();
            var createdAt = IsEditMode ? _createdAt : DateTimeOffset.UtcNow;
            var movie = new Movie(movieId, Title.Trim(), year, createdAt);

            await _repository.UpsertMovieAsync(movie, CancellationToken.None).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("..")).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    Task CancelAsync()
        => Shell.Current.GoToAsync("..");
}

