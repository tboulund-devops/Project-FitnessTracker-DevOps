using FitnessTracker.UI.Service;

namespace FitnessTracker.UI.ViewModels;

public class MainWindowVM:Bindable
{
    // Navigation store that maintains the current application state and view model
    private readonly NavigationStore _navigationStore;
    /// <summary>
    /// Gets the current view model being displayed in the main window
    /// Returns the current view model cast as Bindable for data binding
    /// </summary>
    public Bindable? CurrentViewModel => _navigationStore.CurrentViewModel as Bindable;

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel
    /// Sets up the application infrastructure and initial state
    /// </summary>
    public MainWindowVM()
    {
        // Create shared services and navigation pieces
        _navigationStore = new NavigationStore();
        var navService = new NavigationService(_navigationStore);
        var apiService = new APIService();

        // set initial viewmodel -> Login
        _navigationStore.CurrentViewModel = new LoginPageVM(navService, apiService);

        // subscribe to property changed on store to forward notifications
        // This ensures the UI updates when the current view model changes
        _navigationStore.PropertyChanged += (s, e) =>
        {
            // When the CurrentViewModel property changes in the navigation store,
            // notify the UI that this ViewModel's CurrentViewModel property has also changed
            if (e.PropertyName == nameof(NavigationStore.CurrentViewModel))
                propertyIsChanged(nameof(CurrentViewModel));
        };
    }
}