using Dadstart.Labs.Blutracker.ViewModels;

namespace Dadstart.Labs.Blutracker.Pages;

public partial class AddMoviePage : ContentPage, IQueryAttributable
{
    readonly AddMovieViewModel _viewModel;

    public AddMoviePage(AddMovieViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("id", out var raw))
            return;

        if (raw is string text && Guid.TryParse(text, out var parsed))
            _ = _viewModel.LoadAsync(parsed);
    }
}

