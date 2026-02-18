using Frontend.Service;

namespace Frontend.ViewModels;

public class DashboardPageVM:Bindable
{
    // Service dependencies for API operations and navigation
    private readonly IAPIService _trainerService;
    private readonly INavigationService _navigationService;

    public DashboardPageVM(NavigationService navigationService, IAPIService trainerService)
    {
        _navigationService = navigationService;
        _trainerService = trainerService;

    }
}
