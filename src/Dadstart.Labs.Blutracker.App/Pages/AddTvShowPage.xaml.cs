using Dadstart.Labs.Blutracker.ViewModels;

namespace Dadstart.Labs.Blutracker.Pages;

public partial class AddTvShowPage : ContentPage
{
    public AddTvShowPage(AddTvShowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

