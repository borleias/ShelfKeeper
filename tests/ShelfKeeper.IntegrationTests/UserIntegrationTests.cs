// <copyright file="UserIntegrationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net.Http.Json;
using System.Net;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Application.Services.Users;
using Microsoft.Extensions.DependencyInjection;
using ShelfKeeper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ShelfKeeper.IntegrationTests
{
    public class UserIntegrationTests : IntegrationTestBase
    {
        public UserIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnSuccess()
        {
            // Arrange
            await ResetDatabaseAsync();
            var command = new CreateUserCommand("test@example.com", "Password123!", "Test User");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/register", command);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>();
            Assert.NotNull(result);
            Assert.Equal(command.Email, result.Email);
        }

        [Fact]
        public async Task LoginUser_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("loginuser@example.com", "Password123!", "Login User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            var loginQuery = new LoginUserQuery("loginuser@example.com", "Password123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginUserResponse>();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task LoginUser_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetDatabaseAsync();
            var loginQuery = new LoginUserQuery("nonexistent@example.com", "WrongPassword!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WithValidCredentials_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("change@example.com", "OldPassword123!", "Change User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            var loginResponse = await AuthenticateAsUserAsync("change@example.com", "OldPassword123!");

            var changePasswordCommand = new ChangePasswordCommand(loginResponse.UserId, "OldPassword123!", "NewPassword123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/change-password", changePasswordCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidOldPassword_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("changefail@example.com", "OldPassword123!", "Change Fail User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            var loginResponse = await AuthenticateAsUserAsync("changefail@example.com", "OldPassword123!");

            var changePasswordCommand = new ChangePasswordCommand(loginResponse.UserId, "WrongOldPassword!", "NewPassword123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/change-password", changePasswordCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("forgot@example.com", "Password123!", "Forgot User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            var forgotPasswordCommand = new ForgotPasswordCommand("forgot@example.com");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/forgot-password", forgotPasswordCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ResetPasswordWithToken_WithValidToken_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("reset@example.com", "OldPassword123!", "Reset User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            // Simulate forgot password to get a token
            var forgotPasswordCommand = new ForgotPasswordCommand("reset@example.com");
            await _client.PostAsJsonAsync("/api/v1/users/forgot-password", forgotPasswordCommand);

            // In a real scenario, you'd retrieve the token from the database or email.
            // For integration test, we need to fetch it directly.
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await dbContext.Users.SingleAsync(u => u.Email == "reset@example.com");
                var resetToken = user.PasswordResetToken;

                var resetPasswordCommand = new ResetPasswordWithTokenCommand(resetToken, "reset@example.com", "NewPassword123!");

                // Act
                var response = await _client.PostAsJsonAsync("/api/v1/users/reset-password-with-token", resetPasswordCommand);

                // Assert
                response.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Verify new password works
                var loginQuery = new LoginUserQuery("reset@example.com", "NewPassword123!");
                var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
                loginResponse.EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public async Task ResetPasswordWithToken_WithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("invalidtoken@example.com", "Password123!", "Invalid Token User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            var resetPasswordCommand = new ResetPasswordWithTokenCommand("wrong_token", "invalidtoken@example.com", "NewPassword123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/reset-password-with-token", resetPasswordCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var registerCommand = new CreateUserCommand("delete@example.com", "Password123!", "Delete User");
            await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);

            var loginResponse = await AuthenticateAsUserAsync("delete@example.com", "Password123!");

            // Act
            var response = await _client.DeleteAsync($"/api/v1/users/me");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify user is deleted
            var loginQuery = new LoginUserQuery("delete@example.com", "Password123!");
            var loginResponseAfterDelete = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponseAfterDelete.StatusCode);
        }
    }
}
