using Backend.Domain;
using Xunit;

namespace UnitTests.Backend_UnitTest.LoginTests;

public class LoginRequestTests
    {
        [Fact]
        public void LoginRequest_DefaultConstructor_InitializesPropertiesToNull()
        {
            // Arrange & Act
            var loginRequest = new LoginRequest();

            // Assert
            Assert.Null(loginRequest.Username);
            Assert.Null(loginRequest.Password);
        }

        [Fact]
        public void LoginRequest_CanSetUsernameProperty()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var expectedUsername = "testuser";

            // Act
            loginRequest.Username = expectedUsername;

            // Assert
            Assert.Equal(expectedUsername, loginRequest.Username);
        }

        [Fact]
        public void LoginRequest_CanSetPasswordProperty()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var expectedPassword = "testpassword";

            // Act
            loginRequest.Password = expectedPassword;

            // Assert
            Assert.Equal(expectedPassword, loginRequest.Password);
        }

        [Fact]
        public void LoginRequest_ObjectInitializer_SetsPropertiesCorrectly()
        {
            // Arrange
            var expectedUsername = "john.doe";
            var expectedPassword = "SecurePass123!";

            // Act
            var loginRequest = new LoginRequest
            {
                Username = expectedUsername,
                Password = expectedPassword
            };

            // Assert
            Assert.Equal(expectedUsername, loginRequest.Username);
            Assert.Equal(expectedPassword, loginRequest.Password);
        }

        [Fact]
        public void LoginRequest_CanSetPropertiesAfterInitialization()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "initialuser",
                Password = "initialpass"
            };

            var newUsername = "updateduser";
            var newPassword = "updatedpass";

            // Act
            loginRequest.Username = newUsername;
            loginRequest.Password = newPassword;

            // Assert
            Assert.Equal(newUsername, loginRequest.Username);
            Assert.Equal(newPassword, loginRequest.Password);
        }

        [Fact]
        public void LoginRequest_MultipleInstances_HaveIndependentValues()
        {
            // Arrange
            var request1 = new LoginRequest
            {
                Username = "user1",
                Password = "pass1"
            };

            var request2 = new LoginRequest
            {
                Username = "user2",
                Password = "pass2"
            };

            // Assert
            Assert.NotEqual(request1.Username, request2.Username);
            Assert.NotEqual(request1.Password, request2.Password);
            Assert.Equal("user1", request1.Username);
            Assert.Equal("pass1", request1.Password);
            Assert.Equal("user2", request2.Username);
            Assert.Equal("pass2", request2.Password);
        }

        [Theory]
        [InlineData("admin", "admin123")]
        [InlineData("john.doe@email.com", "P@ssw0rd")]
        [InlineData("test_user", "test")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void LoginRequest_WithVariousInputValues_StoresCorrectly(string username, string password)
        {
            // Arrange & Act
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            // Assert
            Assert.Equal(username, loginRequest.Username);
            Assert.Equal(password, loginRequest.Password);
        }

        [Fact]
        public void LoginRequest_PropertiesAreReadWrite()
        {
            // Arrange
            var loginRequest = new LoginRequest();
            var type = typeof(LoginRequest);

            // Assert
            var usernameProperty = type.GetProperty("Username");
            var passwordProperty = type.GetProperty("Password");

            Assert.NotNull(usernameProperty);
            Assert.NotNull(passwordProperty);
            Assert.True(usernameProperty.CanRead && usernameProperty.CanWrite);
            Assert.True(passwordProperty.CanRead && passwordProperty.CanWrite);
        }
    }