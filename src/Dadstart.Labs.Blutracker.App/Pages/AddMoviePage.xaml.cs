using Dadstart.Labs.Blutracker.ViewModels;

namespace Dadstart.Labs.Blutracker.Pages;

public partial class AddMoviePage : ContentPage
{
    public AddMoviePage(AddMovieViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

