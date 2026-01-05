using Dadstart.Labs.Blutracker.ViewModels;

namespace Dadstart.Labs.Blutracker.Pages;

public partial class AddSeasonPage : ContentPage, IQueryAttributable
{
    readonly AddSeasonViewModel _viewModel;

    public AddSeasonPage(AddSeasonViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tvShowId", out var rawTvShowId) &&
            rawTvShowId is string tvShowIdText &&
            Guid.TryParse(tvShowIdText, out var tvShowId))
            _viewModel.TvShowId = tvShowId;

        if (query.TryGetValue("seasonId", out var rawSeasonId) &&
            rawSeasonId is string seasonIdText &&
            Guid.TryParse(seasonIdText, out var seasonId))
            _ = _viewModel.LoadAsync(seasonId);
    }
}

