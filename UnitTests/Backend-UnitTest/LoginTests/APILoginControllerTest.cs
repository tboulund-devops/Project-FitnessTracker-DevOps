using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.External.APIControllers;
using Backend.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Backend_UnitTest.LoginTests;

public class APILoginControllerTest
{
    private readonly Mock<ILoginService> _mockLoginService;
    private readonly APILoginController _controller;

    public APILoginControllerTest()
    {
        _mockLoginService = new Mock<ILoginService>();
        _controller = new APILoginController(_mockLoginService.Object);
    }

        [Fact]
        public void Constructor_WithValidLoginService_InitializesController()
        {
            // Arrange & Act
            var controller = new APILoginController(_mockLoginService.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullLoginService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new APILoginController(null));
        }

        [Fact]
        public void CheckCredentials_EmptyUsername_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "",
                Password = "testpassword"
            };

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_EmptyPassword_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = ""
            };

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_NullUsername_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = null,
                Password = "testpassword"
            };

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_NullPassword_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = null
            };

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_WhiteSpaceUsername_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "   ",
                Password = "testpassword"
            };

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_WhiteSpacePassword_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "   "
            };

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_NullRequest_ReturnsUnauthorized()
        {
            // Arrange
            LoginRequest request = null;

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credtials Can not be empty", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            _mockLoginService.Setup(x => x.CheckCredentials(request)).Returns(false);

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid username or password", unauthorizedResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(request), Times.Once);
        }

        [Fact]
        public void CheckCredentials_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            _mockLoginService.Setup(x => x.CheckCredentials(request)).Returns(true);

            // Act
            var result = _controller.CheckCredentials(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Valid Credentials", okResult.Value);
            _mockLoginService.Verify(x => x.CheckCredentials(request), Times.Once);
        }

        [Fact]
        public void CheckCredentials_LoginServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "password"
            };

            _mockLoginService.Setup(x => x.CheckCredentials(request))
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _controller.CheckCredentials(request));
            Assert.Equal("Database connection failed", exception.Message);
            _mockLoginService.Verify(x => x.CheckCredentials(request), Times.Once);
        }
}