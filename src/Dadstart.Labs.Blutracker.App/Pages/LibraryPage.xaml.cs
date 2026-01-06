using Dadstart.Labs.Blutracker.ViewModels;

namespace Dadstart.Labs.Blutracker.Pages;

public partial class LibraryPage : ContentPage
{
    readonly LibraryViewModel _viewModel;

    public LibraryPage(LibraryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshCommand.ExecuteAsync(null).ConfigureAwait(false);
    }
}

