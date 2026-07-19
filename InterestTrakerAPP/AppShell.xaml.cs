using InterestTrakerAPP.Views;

namespace InterestTrakerAPP;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AddHoldingPage), typeof(AddHoldingPage));
        Routing.RegisterRoute(nameof(AccountDetailsPage), typeof(AccountDetailsPage));
        Routing.RegisterRoute(nameof(GoalDetailsPage), typeof(GoalDetailsPage));
    }
}