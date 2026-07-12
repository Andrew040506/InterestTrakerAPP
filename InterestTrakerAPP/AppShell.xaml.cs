namespace InterestTrakerAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.AddHoldingPage), typeof(Views.AddHoldingPage));
        }
    }
}
