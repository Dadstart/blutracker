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
    Task EditMovieAsync(Movie movie)
        => Shell.Current.GoToAsync($"{nameof(Pages.AddMoviePage)}?id={movie.Id:D}");

    [RelayCommand]
    async Task DeleteMovieAsync(Movie movie)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _repository.DeleteMovieAsync(movie.Id, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    Task AddTvShowAsync()
        => Shell.Current.GoToAsync(nameof(Pages.AddTvShowPage));

    [RelayCommand]
    Task EditTvShowAsync(TvShow tvShow)
        => Shell.Current.GoToAsync($"{nameof(Pages.AddTvShowPage)}?id={tvShow.Id:D}");

    [RelayCommand]
    async Task DeleteTvShowAsync(TvShow tvShow)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _repository.DeleteTvShowAsync(tvShow.Id, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    Task OpenTvShowAsync(TvShow tvShow)
        => Shell.Current.GoToAsync($"{nameof(Pages.TvShowDetailPage)}?tvShowId={tvShow.Id:D}");
}

