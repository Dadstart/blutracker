using Dadstart.Labs.Blutracker.ViewModels;

namespace Dadstart.Labs.Blutracker.Pages;

public partial class TvShowDetailPage : ContentPage, IQueryAttributable
{
    readonly TvShowDetailViewModel _viewModel;

    public TvShowDetailPage(TvShowDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("tvShowId", out var raw))
            return;

        if (raw is string text && Guid.TryParse(text, out var parsed))
            _ = _viewModel.LoadAsync(parsed);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshCommand.ExecuteAsync(null).ConfigureAwait(false);
    }
}

