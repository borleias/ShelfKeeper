// <copyright file="ErrorHandlingIntegrationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net;
using System.Net.Http.Json;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.IntegrationTests
{
    public class ErrorHandlingIntegrationTests : IntegrationTestBase
    {
        public ErrorHandlingIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task UnauthorizedAccess_ShouldReturn401Unauthorized()
        {
            // Arrange
            await ResetDatabaseAsync();
            // No authentication header set

            // Act
            var response = await _client.GetAsync("/api/v1/mediaitems");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task InvalidInput_ShouldReturn400BadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("invalidinput@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var command = new CreateUserCommand("invalid-email", "short", ""); // Invalid data

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/register", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errors = await response.Content.ReadFromJsonAsync<List<OperationError>>();
            Assert.NotNull(errors);
            Assert.True(errors.Count > 0);
        }

        [Fact]
        public async Task NotFoundResource_ShouldReturn404NotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("notfound@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var nonExistentMediaItemId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/mediaitems/{nonExistentMediaItemId}?userId={user.UserId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ForbiddenAccess_ShouldReturn403Forbidden()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("forbidden@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Try to access an admin endpoint as a regular user
            var response = await _client.GetAsync("/api/v1/admin/users");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Note: Rate limiting tests are harder to implement reliably in integration tests
        // as they depend on timing and external factors. They are often better suited
        // for dedicated performance/load tests or mocked unit tests.

        [Fact]
        public async Task RateLimiting_ShouldReturn429TooManyRequests()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("ratelimit@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Make requests to trigger rate limit (default is 10 requests per 30 seconds)
            for (int i = 0; i < 10; i++)
            {
                var response = await _client.GetAsync("/api/v1/mediaitems");
                response.EnsureSuccessStatusCode(); // First 10 requests should succeed
            }

            // Act: The 11th request should be rate-limited
            var rateLimitedResponse = await _client.GetAsync("/api/v1/mediaitems");

            // Assert
            Assert.Equal(HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode);
        }

        private async Task<LoginUserResponse> RegisterAndLoginUser(string email, string password)
        {
            var registerCommand = new CreateUserCommand(email, password, "Test User");
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);
            registerResponse.EnsureSuccessStatusCode();

            var loginQuery = new LoginUserQuery(email, password);
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
            loginResponse.EnsureSuccessStatusCode();
            return await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        }
    }
}
