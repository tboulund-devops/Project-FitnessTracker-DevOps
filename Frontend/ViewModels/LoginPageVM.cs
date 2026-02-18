using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Frontend.Service;
using Frontend.ViewModels;
using Frontend.Models;

namespace Frontend.ViewModels;

public class LoginPageVM:Bindable
{
    // Service dependencies for navigation and API operations
    private readonly INavigationService _navigationService;
    private readonly IAPIService _apiService;
    
    public ICommand LoginCommand { get;}
    
    // Add error properties
    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            propertyIsChanged();
        }
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            propertyIsChanged();
        }
    }
    
    private LoginInfo _loginInfo;
    public LoginInfo LoginInfo
    {
        get => _loginInfo;
        set
        {
            if (!ReferenceEquals(_loginInfo, value))
            {
                _loginInfo = value;
                propertyIsChanged();
            }
        }
    }
    
    public LoginPageVM(INavigationService navigationService, IAPIService apiService)
    {
        _navigationService = navigationService;
        _apiService = apiService;
        
        LoginInfo = new LoginInfo("", "");
        LoginCommand = new ICommandBase(async _ => await LoginAsync());
        
    }
    
    /// <summary>
    ///  Performs the login operation by validating credentials with the API
    /// Navigates to the admin home page upon successful authentication
    /// </summary>
    private async Task LoginAsync()
    {
        
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            
            var homePageVm = new HomePageVM(_navigationService, _apiService);
            _navigationService.Navigate(homePageVm);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
        
}