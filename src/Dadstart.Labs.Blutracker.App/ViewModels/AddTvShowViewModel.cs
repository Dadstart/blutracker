using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class AddTvShowViewModel : ObservableObject
{
    readonly IMediaRepository _repository;
    Guid? _tvShowId;
    DateTimeOffset _createdAt;

    [ObservableProperty]
    string _title = string.Empty;

    [ObservableProperty]
    bool _isBusy;

    public AddTvShowViewModel(IMediaRepository repository) => _repository = repository;

    public bool IsEditMode => _tvShowId is not null;

    public string PageTitle => IsEditMode ? "Edit TV show" : "Add TV show";

    public async Task LoadAsync(Guid id)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var show = await _repository.GetTvShowAsync(id, CancellationToken.None).ConfigureAwait(false);
            if (show is null)
                return;

            _tvShowId = show.Id;
            _createdAt = show.CreatedAt;
            Title = show.Title;

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

            var tvShowId = _tvShowId ?? Guid.NewGuid();
            var createdAt = IsEditMode ? _createdAt : DateTimeOffset.UtcNow;
            var show = new TvShow(tvShowId, Title.Trim(), createdAt);
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

