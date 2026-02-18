using System.Windows.Input;
using Frontend.Models;
using Frontend.Service;
using Frontend.ViewModels;
using Xunit;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;

namespace UnitTests.Frontend_UnitTests;

public class LoginVMTests
{
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IAPIService> _mockApiService;
    private readonly LoginPageVM _viewModel;

    public LoginVMTests()
    {
        _mockNavigationService = new Mock<INavigationService>();
        _mockApiService = new Mock<IAPIService>();
        _viewModel = new LoginPageVM(_mockNavigationService.Object, _mockApiService.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.LoginInfo.Should().NotBeNull();
        _viewModel.LoginInfo.Username.Should().Be("");
        _viewModel.LoginInfo.Password.Should().Be("");
        _viewModel.LoginCommand.Should().NotBeNull();
        _viewModel.LoginCommand.Should().BeAssignableTo<ICommandBase>();
    }

    [Fact]
    public void LoginInfo_Setter_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) => 
        {
            if (args.PropertyName == nameof(LoginPageVM.LoginInfo))
                propertyChangedRaised = true;
        };

        var newLoginInfo = new LoginInfo("test@example.com", "password123");

        // Act
        _viewModel.LoginInfo = newLoginInfo;

        // Assert
        propertyChangedRaised.Should().BeTrue();
        _viewModel.LoginInfo.Should().BeSameAs(newLoginInfo);
    }

    [Fact]
    public void LoginInfo_Setter_WithSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) => 
        {
            if (args.PropertyName == nameof(LoginPageVM.LoginInfo))
                propertyChangedRaised = true;
        };

        // Act
        _viewModel.LoginInfo = _viewModel.LoginInfo; // Set to same value

        // Assert
        propertyChangedRaised.Should().BeFalse();
    }

    [Fact]
    public void LoginCommand_Execute_ShouldNavigateToHomePage()
    {
        // Arrange
        HomePageVM capturedHomePageVm = null;
        _mockNavigationService
            .Setup(x => x.Navigate(It.IsAny<HomePageVM>()))
            .Callback<object>(vm => capturedHomePageVm = vm as HomePageVM);

        // Act
        _viewModel.LoginCommand.Execute(null);

        // Assert
        _mockNavigationService.Verify(
            x => x.Navigate(It.IsAny<HomePageVM>()), 
            Times.Once);
        
        capturedHomePageVm.Should().NotBeNull();
        capturedHomePageVm.Should().BeOfType<HomePageVM>();
    }

    [Fact]
    public void LoginCommand_Execute_ShouldPassSameServicesToHomePageVM()
    {
        // Arrange
        HomePageVM capturedHomePageVm = null;
        _mockNavigationService
            .Setup(x => x.Navigate(It.IsAny<HomePageVM>()))
            .Callback<object>(vm => capturedHomePageVm = vm as HomePageVM);

        // Act
        _viewModel.LoginCommand.Execute(null);

        // Assert
        capturedHomePageVm.Should().NotBeNull();
        
        // Verify that HomePageVM receives the same service instances
        var homePageVmType = capturedHomePageVm.GetType();
        var navigationServiceField = homePageVmType.GetField("_navigationService", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var apiServiceField = homePageVmType.GetField("_apiService", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var actualNavigationService = navigationServiceField?.GetValue(capturedHomePageVm);
        var actualApiService = apiServiceField?.GetValue(capturedHomePageVm);

        actualNavigationService.Should().BeSameAs(_mockNavigationService.Object);
        actualApiService.Should().BeSameAs(_mockApiService.Object);
    }
    
    [Fact]
    public async Task LoginCommand_Execute_ShouldSetErrorStateWhenNavigationFails()
    {
        // Arrange
        _mockNavigationService
            .Setup(x => x.Navigate(It.IsAny<HomePageVM>()))
            .Throws(new System.Exception("Navigation failed"));

        // Act
        _viewModel.LoginCommand.Execute(null);
    
        // Wait for async operation to complete
        await Task.Delay(100); // Simple wait
        // Or better: wait for IsLoading to become false
        await WaitForCondition(() => !_viewModel.IsLoading, TimeSpan.FromSeconds(2));

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Navigation failed");
    }
    

    [Fact]
    public void LoginInfo_ShouldAllowUpdatingProperties()
    {
        // Arrange
        var userName = "user@example.com";
        var password = "secure123";

        // Act
        _viewModel.LoginInfo.Username = userName;
        _viewModel.LoginInfo.Password = password;

        // Assert
        _viewModel.LoginInfo.Username.Should().Be(userName);
        _viewModel.LoginInfo.Password.Should().Be(password);
    }
    
    // Helper method to wait for conditions
    private async Task WaitForCondition(Func<bool> condition, TimeSpan timeout)
    {
        var start = DateTime.Now;
        while (!condition() && DateTime.Now - start < timeout)
        {
            await Task.Delay(50);
        }
    }
}
