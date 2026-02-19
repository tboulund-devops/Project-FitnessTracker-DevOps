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
    public async Task LoginCommand_Execute_ShouldNavigateToHomePage()
    {
        // Arrange
        _viewModel.LoginInfo = new LoginInfo("admin", "1234");

        _mockApiService
            .Setup(x => x.LoginAsync("admin", "1234"))
            .ReturnsAsync(true);

        HomePageVM capturedHomePageVm = null;

        _mockNavigationService
            .Setup(x => x.Navigate(It.IsAny<Bindable>()))
            .Callback<Bindable>(vm => capturedHomePageVm = vm as HomePageVM);

        // Act
        _viewModel.LoginCommand.Execute(null);

        await WaitForCondition(() => !_viewModel.IsLoading, TimeSpan.FromSeconds(2));

        // Assert
        _mockNavigationService.Verify(
            x => x.Navigate(It.IsAny<Bindable>()),
            Times.Once);

        capturedHomePageVm.Should().NotBeNull();
        capturedHomePageVm.Should().BeOfType<HomePageVM>();
    }



    [Fact]
    public async Task LoginCommand_Execute_ShouldPassSameServicesToHomePageVM()
    {
        // Arrange
        _viewModel.LoginInfo = new LoginInfo("admin", "1234");

        _mockApiService
            .Setup(x => x.LoginAsync("admin", "1234"))
            .ReturnsAsync(true);

        HomePageVM capturedHomePageVm = null;

        _mockNavigationService
            .Setup(x => x.Navigate(It.IsAny<Bindable>()))
            .Callback<Bindable>(vm => capturedHomePageVm = vm as HomePageVM);

        // Act
        _viewModel.LoginCommand.Execute(null);

        await WaitForCondition(() => !_viewModel.IsLoading, TimeSpan.FromSeconds(2));

        // Assert
        capturedHomePageVm.Should().NotBeNull();

        var homePageVmType = capturedHomePageVm.GetType();

        var navigationServiceField = homePageVmType.GetField(
            "_navigationService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var apiServiceField = homePageVmType.GetField(
            "_apiService",
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
        _viewModel.LoginInfo = new LoginInfo("admin", "1234");

        _mockApiService
            .Setup(x => x.LoginAsync("admin", "1234"))
            .ReturnsAsync(true);

        _mockNavigationService
            .Setup(x => x.Navigate(It.IsAny<Bindable>()))
            .Throws(new Exception("Navigation failed"));

        // Act
        _viewModel.LoginCommand.Execute(null);

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
    
    // Test Empty Credentials
    [Fact]
    public async Task LoginCommand_Should_SetError_When_Credentials_Are_Empty()
    {
        // Arrange
        var navigationMock = new Mock<INavigationService>();
        var apiMock = new Mock<IAPIService>();

        var vm = new LoginPageVM(navigationMock.Object, apiMock.Object);
        vm.LoginInfo = new LoginInfo("", "");

        // Act
        vm.LoginCommand.Execute(null);
        
        // Assert
        Assert.Equal("Login credentials cannot be empty", vm.ErrorMessage);
        Assert.False(vm.HasError);
        apiMock.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        navigationMock.Verify(x => x.Navigate(It.IsAny<Bindable>()), Times.Never);

    }
    
    // Test successful login
    [Fact]
    public async Task LoginCommand_Should_Navigate_When_Login_Is_Valid()
    {
        // Arrange
        var navigationMock = new Mock<INavigationService>();
        var apiMock = new Mock<IAPIService>();

        apiMock
            .Setup(x => x.LoginAsync("admin", "1234"))
            .ReturnsAsync(true);

        var vm = new LoginPageVM(navigationMock.Object, apiMock.Object);
        vm.LoginInfo = new LoginInfo("admin", "1234");

        // Act
        vm.LoginCommand.Execute(null);
        await Task.Yield();
        
        // Assert
        navigationMock.Verify(x => x.Navigate(It.IsAny<HomePageVM>()), Times.Once);
        Assert.False(vm.IsLoading);
        Assert.False(vm.HasError);
    }

    //Test invalid Credentials
    [Fact]
    public async Task LoginCommand_Should_SetError_When_Login_Fails()
    {
        // Arrange
        var navigationMock = new Mock<INavigationService>();
        var apiMock = new Mock<IAPIService>();

        apiMock
            .Setup(x => x.LoginAsync("admin", "wrong"))
            .ReturnsAsync(false);

        var vm = new LoginPageVM(navigationMock.Object, apiMock.Object);
        vm.LoginInfo = new LoginInfo("admin", "wrong");

        // Act
        vm.LoginCommand.Execute(null);
        await Task.Yield();



        // Assert
        Assert.Equal("Login failed", vm.ErrorMessage);
        Assert.True(vm.HasError);
        navigationMock.Verify(x => x.Navigate(It.IsAny<Bindable>()), Times.Never);

    }
    
    //Test API Throws Exception
    [Fact]
    public async Task LoginCommand_Should_SetError_When_Api_Throws_Exception()
    {
        // Arrange
        var navigationMock = new Mock<INavigationService>();
        var apiMock = new Mock<IAPIService>();

        apiMock
            .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Server error"));

        var vm = new LoginPageVM(navigationMock.Object, apiMock.Object);
        vm.LoginInfo = new LoginInfo("admin", "1234");

        // Act
        vm.LoginCommand.Execute(null);
        await Task.Yield();

        // Assert
        Assert.True(vm.HasError);
        Assert.Equal("Server error", vm.ErrorMessage);
        Assert.False(vm.IsLoading);
    }
    
    //Test LoginInfo Setter Change
    [Fact]
    public void LoginInfo_Should_Update_When_New_Instance_Assigned()
    {
        // Arrange
        var navigationMock = new Mock<INavigationService>();
        var apiMock = new Mock<IAPIService>();
        var vm = new LoginPageVM(navigationMock.Object, apiMock.Object);

        var newLoginInfo = new LoginInfo("user", "pass");

        // Act
        vm.LoginInfo = newLoginInfo;

        // Assert
        Assert.Equal("user", vm.LoginInfo.Username);
    }
    
    //Test IsLoading Gets Set During Execution
    [Fact]
    public async Task LoginCommand_Should_Set_IsLoading_During_Execution()
    {
        // Arrange
        var navigationMock = new Mock<INavigationService>();
        var apiMock = new Mock<IAPIService>();

        var tcs = new TaskCompletionSource<bool>();

        apiMock
            .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(tcs.Task);

        var vm = new LoginPageVM(navigationMock.Object, apiMock.Object);
        vm.LoginInfo = new LoginInfo("admin", "1234");

        // Act
        vm.LoginCommand.Execute(null);

        // Assert
        Assert.True(vm.IsLoading);

        tcs.SetResult(true);    

        Assert.False(vm.IsLoading);
    }


}
