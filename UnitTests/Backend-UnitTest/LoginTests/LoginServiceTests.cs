using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Backend.Gateway;
using Backend.Service;
using Moq;
using Xunit;

namespace UnitTests.Backend_UnitTest.LoginTests
{
    public class LoginServiceTests
    {
        private readonly Mock<ILoginRepo> _mockRepo;
        private readonly LoginService _loginService;

        public LoginServiceTests()
        {
            _mockRepo = new Mock<ILoginRepo>();
            _loginService = new LoginService(_mockRepo.Object);
        }

        [Fact]
        public void Constructor_WithValidRepo_InitializesService()
        {
            // Arrange & Act
            var service = new LoginService(_mockRepo.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullRepo_ThrowsArgumentNullException()
        {
            // Arrange
            ILoginRepo nullRepo = null;

            // Act & Assert - Method 1: Using Assert.Throws
            var exception = Assert.Throws<ArgumentNullException>(() => new LoginService(nullRepo));
            Assert.Equal("repo", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullRepo_ThrowsArgumentNullException_ExplicitTryCatch()
        {
            // Arrange
            ILoginRepo nullRepo = null;

            // Act & Assert - Method 2: Using try-catch for debugging
            try
            {
                var service = new LoginService(nullRepo);
                Assert.Fail("Expected ArgumentNullException was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.Equal("repo", ex.ParamName);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected ArgumentNullException but got {ex.GetType().Name}");
            }
        }
        [Fact]
        public void CheckCredentials_ValidCredentials_ReturnsTrue()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_InvalidUsername_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = "somepassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_EmptyUsername_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "",
                Password = "somepassword"
            };

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            // Verify the repository was NOT called for empty username
            _mockRepo.Verify(x => x.getCredentials(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CheckCredentials_EmptyPassword_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = ""
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_NullUsername_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = null,
                Password = "somepassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(null)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(null), Times.Once);
        }

        [Fact]
        public void CheckCredentials_NullPassword_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = null
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_CaseSensitiveUsername_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "TestUser", // Different case
                Password = "correctpassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_CaseSensitivePassword_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "CorrectPassword" // Different case
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_WhitespaceInUsername_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser ",
                Password = "correctpassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_WhitespaceInPassword_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword "
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_RepoReturnsEmptyList_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            var dbCredentials = new List<string>(); // Empty list
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_RepoReturnsListWithInsufficientElements_ReturnsFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            var dbCredentials = new List<string> { "onlyusername" }; // Only one element
            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns(dbCredentials);

            // Act
            var result = _loginService.CheckCredentials(request);

            // Assert
            Assert.False(result);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_RepoReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            _mockRepo.Setup(x => x.getCredentials(request.Username)).Returns((List<string>)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _loginService.CheckCredentials(request));
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_RepoThrowsException_PropagatesException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            _mockRepo.Setup(x => x.getCredentials(request.Username))
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _loginService.CheckCredentials(request));
            Assert.Equal("Database connection failed", exception.Message);
            _mockRepo.Verify(x => x.getCredentials(request.Username), Times.Once);
        }

        [Fact]
        public void CheckCredentials_MultipleCalls_ReturnsAppropriateResults()
        {
            // Arrange
            var validRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            var invalidRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var dbCredentials = new List<string> { "testuser", "correctpassword" };
            _mockRepo.Setup(x => x.getCredentials(It.IsAny<string>())).Returns(dbCredentials);

            // Act
            var validResult = _loginService.CheckCredentials(validRequest);
            var invalidResult = _loginService.CheckCredentials(invalidRequest);

            // Assert
            Assert.True(validResult);
            Assert.False(invalidResult);
            _mockRepo.Verify(x => x.getCredentials(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}