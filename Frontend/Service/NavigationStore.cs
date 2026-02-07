using System.ComponentModel;

namespace FitnessTracker.UI.Service;

/// <summary>
/// Stores and manages the current view model state for navigation
/// Implements INotifyPropertyChanged to notify views of changes
/// </summary>
public class NavigationStore:INotifyPropertyChanged
{
    // Backing field for the current view model
    private object? _currentViewModel;
    /// <summary>
    /// Gets or sets the current view model being displayed
    /// Raises PropertyChanged event when the value changes
    /// </summary>
    public object? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            if (_currentViewModel == value) return;
            _currentViewModel = value;
            // Notify subscribers that the CurrentViewModel property has changed
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
        }
    }
    /// <summary>
    /// Event that is raised when a property value changes
    /// Used by the UI to update when the current view model changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    
}