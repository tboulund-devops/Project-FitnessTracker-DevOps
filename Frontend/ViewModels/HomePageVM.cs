using Frontend.Service;

namespace Frontend.ViewModels;

public class HomePageVM:Bindable
{
    // Service dependencies for API operations and navigation
    private readonly IAPIService _apiService;
    private readonly INavigationService _navigationService;
    
    public HomePageVM(INavigationService navigationService, IAPIService apiService)
    {
        _navigationService = navigationService;
        _apiService = apiService;

    }
}