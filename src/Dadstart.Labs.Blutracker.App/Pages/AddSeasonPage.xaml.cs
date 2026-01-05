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
        if (!query.TryGetValue("tvShowId", out var raw))
            return;

        if (raw is string text && Guid.TryParse(text, out var parsed))
            _viewModel.TvShowId = parsed;
    }
}

