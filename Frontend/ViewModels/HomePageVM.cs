using FitnessTracker.UI.Service;

namespace FitnessTracker.UI.ViewModels;

public class HomePageVM:Bindable
{
    // Service dependencies for API operations and navigation
    private readonly IAPIService _trainerService;
    private readonly NavigationService _navigationService;
    
    public HomePageVM(NavigationService navigationService, IAPIService trainerService)
    {
        _navigationService = navigationService;
        _trainerService = trainerService;

    }
}