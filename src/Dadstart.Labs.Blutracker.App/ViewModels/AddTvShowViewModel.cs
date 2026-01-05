using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class AddTvShowViewModel : ObservableObject
{
    readonly IMediaRepository _repository;

    [ObservableProperty]
    string _title = string.Empty;

    [ObservableProperty]
    bool _isBusy;

    public AddTvShowViewModel(IMediaRepository repository) => _repository = repository;

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

            var show = new TvShow(Guid.NewGuid(), Title.Trim(), DateTimeOffset.UtcNow);
            await _repository.UpsertTvShowAsync(show, CancellationToken.None).ConfigureAwait(false);
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

