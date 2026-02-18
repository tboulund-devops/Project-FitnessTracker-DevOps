using Frontend.ViewModels;

namespace Frontend.Service;

/// <summary>
/// /// Service for handling navigation between different views in the application
/// Manages the current view model displayed in the main window
/// </summary>
public class NavigationService
{
    // Reference to the navigation store that maintains the current view state
    private readonly NavigationStore _navigationStore;
    /// <summary>
    /// Initializes a new instance of the NavigationService
    /// </summary>
    /// <param name="navigationStore"></param>
    public NavigationService(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
    }

    /// <summary>
    /// Navigates to a new view by setting the current view model in the navigation store
    /// </summary>
    /// <param name="viewModel"></param>
    public void Navigate(Bindable viewModel)
    {
        _navigationStore.CurrentViewModel = viewModel;
    }
    
}