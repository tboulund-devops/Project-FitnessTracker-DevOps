using Frontend.ViewModels;

namespace Frontend.Service;

public interface INavigationService
{
    public void Navigate(Bindable viewModel);

}