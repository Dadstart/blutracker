using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class AddSeasonViewModel : ObservableObject
{
    readonly IMediaRepository _repository;

    [ObservableProperty]
    Guid _tvShowId;

    [ObservableProperty]
    string _seasonNumber = string.Empty;

    [ObservableProperty]
    string _episodeCount = string.Empty;

    [ObservableProperty]
    bool _isBusy;

    public AddSeasonViewModel(IMediaRepository repository) => _repository = repository;

    [RelayCommand]
    async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (!int.TryParse(SeasonNumber, out var seasonNumber) || seasonNumber < 1)
            return;

        try
        {
            IsBusy = true;

            int? episodeCount = int.TryParse(EpisodeCount, out var parsed) ? parsed : null;
            var season = new Season(Guid.NewGuid(), TvShowId, seasonNumber, episodeCount, DateTimeOffset.UtcNow);

            await _repository.UpsertSeasonAsync(season, CancellationToken.None).ConfigureAwait(false);
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

