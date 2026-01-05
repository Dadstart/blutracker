namespace Dadstart.Labs.Blutracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Pages.AddMoviePage), typeof(Pages.AddMoviePage));
        Routing.RegisterRoute(nameof(Pages.AddTvShowPage), typeof(Pages.AddTvShowPage));
        Routing.RegisterRoute(nameof(Pages.AddSeasonPage), typeof(Pages.AddSeasonPage));
    }
}

