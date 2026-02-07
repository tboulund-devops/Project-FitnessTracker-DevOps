using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FitnessTracker.UI.ViewModels;

/// <summary>
/// Base class for all view models that require property change notification
/// Implements INotifyPropertyChanged interface to support data binding in the UI
/// </summary>
public class Bindable : INotifyPropertyChanged
{
    /// <summary>
    /// Event that is raised when a property value changes
    /// Used by the UI framework to update bound controls automatically
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event to notify the UI that a property has changed
    /// Uses CallerMemberName attribute to automatically get the calling property name
    /// </summary>
    /// <param name="memberName"></param>
    protected void propertyIsChanged([CallerMemberName] string memberName = "")
    {
        // Invoke the PropertyChanged event if there are subscribers
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }

    // Backing field for the IsLoading property
    private bool _isLoading;

    /// <summary>
    /// Gets or sets a value indicating whether the view model is currently performing an operation
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            propertyIsChanged();
        }
    }
}