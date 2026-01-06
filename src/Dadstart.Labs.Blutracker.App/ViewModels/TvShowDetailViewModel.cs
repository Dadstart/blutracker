using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class TvShowDetailViewModel : ObservableObject
{
    readonly IMediaRepository _repository;

    public ObservableCollection<Season> Seasons { get; } = new();

    [ObservableProperty]
    bool _isBusy;

    [ObservableProperty]
    Guid _tvShowId;

    [ObservableProperty]
    string _title = string.Empty;

    public TvShowDetailViewModel(IMediaRepository repository) => _repository = repository;

    public async Task LoadAsync(Guid tvShowId)
    {
        TvShowId = tvShowId;
        await RefreshAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    async Task RefreshAsync()
    {
        if (IsBusy || TvShowId == Guid.Empty)
            return;

        try
        {
            IsBusy = true;

            var show = await _repository.GetTvShowAsync(TvShowId, CancellationToken.None).ConfigureAwait(false);
            Title = show?.Title ?? "TV show";

            Seasons.Clear();
            foreach (var season in await _repository.ListSeasonsAsync(TvShowId, CancellationToken.None).ConfigureAwait(false))
                Seasons.Add(season);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    Task AddSeasonAsync()
        => Shell.Current.GoToAsync($"{nameof(Pages.AddSeasonPage)}?tvShowId={TvShowId:D}");

    [RelayCommand]
    Task EditSeasonAsync(Season season)
        => Shell.Current.GoToAsync($"{nameof(Pages.AddSeasonPage)}?tvShowId={TvShowId:D}&seasonId={season.Id:D}");

    [RelayCommand]
    async Task DeleteSeasonAsync(Season season)
    {
        if (IsBusy)
            return;

        var confirmed = await ConfirmAsync("Delete season?", $"Delete season {season.SeasonNumber}?").ConfigureAwait(false);
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _repository.DeleteSeasonAsync(season.Id, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    Task EditTvShowAsync()
        => Shell.Current.GoToAsync($"{nameof(Pages.AddTvShowPage)}?id={TvShowId:D}");

    [RelayCommand]
    async Task DeleteTvShowAsync()
    {
        if (IsBusy || TvShowId == Guid.Empty)
            return;

        var confirmed = await ConfirmAsync("Delete TV show?", $"Delete \"{Title}\" and all seasons?").ConfigureAwait(false);
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _repository.DeleteTvShowAsync(TvShowId, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("..")).ConfigureAwait(false);
    }

    static Task<bool> ConfirmAsync(string title, string message)
        => MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var app = Application.Current;
            if (app?.Windows.Count == 0)
                return false;

            var page = app?.Windows[0].Page;
            if (page is null)
                return false;

            return await page.DisplayAlertAsync(title, message, "Delete", "Cancel").ConfigureAwait(false);
        });
}

