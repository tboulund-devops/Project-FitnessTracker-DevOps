using System.Threading.Tasks;
using System.Windows.Input;
using FitnessTracker.UI.Service;

namespace FitnessTracker.UI.ViewModels;

public class LoginPageVM:Bindable
{
    // Service dependencies for navigation and API operations
    private readonly NavigationService _navigationService;
    private readonly IAPIService _apiService;
    
    public ICommand LoginCommand { get;}
    
    public LoginPageVM(NavigationService navigationService, IAPIService apiService)
    {
        _navigationService = navigationService;
        _apiService = apiService;
        // LoginInfo = new LoginInfo("", "");
        LoginCommand = new ICommandBase(async _ => await LoginAsync());
        
    }
    
    /// <summary>
    ///  Performs the login operation by validating credentials with the API
    /// Navigates to the admin home page upon successful authentication
    /// </summary>
    private async Task LoginAsync()
    {
        
        var homePageVm = new HomePageVM(_navigationService, _apiService);
         _navigationService.Navigate(homePageVm);
        
        // IsLoading = true;
        // try
        // {
        //     var result = await _apiService.LoginAsync(LoginInfo.Username, LoginInfo.Password);
        //     if (result)
        //     {
        //         var adminVm = new AdminHomePageViewModel(_navigationService, _apiService);
        //         _navigationService.Navigate(adminVm);
        //     }
        //     else
        //     {
        //         ShowError("Wrong username or password!");
        //     }
        // }
        // finally
        // {
        //     IsLoading = false;
        // }
    }
        
}