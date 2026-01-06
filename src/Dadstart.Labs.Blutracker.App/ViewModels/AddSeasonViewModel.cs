using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class AddSeasonViewModel : ObservableObject
{
    readonly IMediaRepository _repository;
    Guid? _seasonId;
    DateTimeOffset _createdAt;

    [ObservableProperty]
    Guid _tvShowId;

    [ObservableProperty]
    string _seasonNumber = string.Empty;

    [ObservableProperty]
    string _episodeCount = string.Empty;

    [ObservableProperty]
    bool _isBusy;

    public AddSeasonViewModel(IMediaRepository repository) => _repository = repository;

    public bool IsEditMode => _seasonId is not null;

    public string PageTitle => IsEditMode ? "Edit season" : "Add season";

    public async Task LoadAsync(Guid seasonId)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var season = await _repository.GetSeasonAsync(seasonId, CancellationToken.None).ConfigureAwait(false);
            if (season is null)
                return;

            _seasonId = season.Id;
            _createdAt = season.CreatedAt;
            TvShowId = season.TvShowId;
            SeasonNumber = season.SeasonNumber.ToString();
            EpisodeCount = season.EpisodeCount?.ToString() ?? string.Empty;

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

        if (!int.TryParse(SeasonNumber, out var seasonNumber) || seasonNumber < 1)
            return;

        try
        {
            IsBusy = true;

            int? episodeCount = int.TryParse(EpisodeCount, out var parsed) ? parsed : null;
            var seasonId = _seasonId ?? Guid.NewGuid();
            var createdAt = IsEditMode ? _createdAt : DateTimeOffset.UtcNow;
            var season = new Season(seasonId, TvShowId, seasonNumber, episodeCount, createdAt);

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

