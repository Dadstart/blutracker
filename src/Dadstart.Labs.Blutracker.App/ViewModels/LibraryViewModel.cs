using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Blutracker.Models;
using Dadstart.Labs.Blutracker.Storage;

namespace Dadstart.Labs.Blutracker.ViewModels;

public sealed partial class LibraryViewModel : ObservableObject
{
    readonly IMediaRepository _repository;

    public ObservableCollection<Movie> Movies { get; } = new();
    public ObservableCollection<TvShow> TvShows { get; } = new();

    [ObservableProperty]
    bool _isBusy;

    public LibraryViewModel(IMediaRepository repository) => _repository = repository;

    [RelayCommand]
    async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Movies.Clear();
            foreach (var movie in await _repository.ListMoviesAsync(CancellationToken.None).ConfigureAwait(false))
                Movies.Add(movie);

            TvShows.Clear();
            foreach (var show in await _repository.ListTvShowsAsync(CancellationToken.None).ConfigureAwait(false))
                TvShows.Add(show);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    Task AddMovieAsync()
        => Shell.Current.GoToAsync(nameof(Pages.AddMoviePage));

    [RelayCommand]
    Task AddTvShowAsync()
        => Shell.Current.GoToAsync(nameof(Pages.AddTvShowPage));

    [RelayCommand]
    Task AddSeasonAsync(Guid tvShowId)
        => Shell.Current.GoToAsync($"{nameof(Pages.AddSeasonPage)}?tvShowId={tvShowId:D}");
}

