using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class AddMovieViewModel : ObservableObject
{
    readonly IMediaRepository _repository;

    [ObservableProperty]
    string _title = string.Empty;

    [ObservableProperty]
    string _releaseYear = string.Empty;

    [ObservableProperty]
    bool _isBusy;

    public AddMovieViewModel(IMediaRepository repository) => _repository = repository;

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
            var movie = new Movie(Guid.NewGuid(), Title.Trim(), year, DateTimeOffset.UtcNow);

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

