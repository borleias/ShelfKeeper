// <copyright file="AdminIntegrationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net.Http.Json;
using System.Net;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.IntegrationTests
{
    public class AdminIntegrationTests : IntegrationTestBase
    {
        public AdminIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetAllUsers_AsAdmin_ShouldReturnAllUsers()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/admin/users");

            // Assert
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.NotNull(users);
            Assert.True(users.Count >= 1); // At least the admin user
        }

        [Fact]
        public async Task GetAllUsers_AsRegularUser_ShouldReturnForbidden()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsUserAsync("alice@example.com", "password123"); // Assuming alice is a regular user

            // Act
            var response = await _client.GetAsync("/api/v1/admin/users");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetUserById_AsAdmin_ShouldReturnUser()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            // Assuming a user with ID exists from seed data or previous test
            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            var userIdToFetch = allUsers.First(u => u.Email == "alice@example.com").UserId;

            // Act
            var response = await _client.GetAsync($"/api/v1/admin/users/{userIdToFetch}");

            // Assert
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(user);
            Assert.Equal(userIdToFetch, user.UserId);
        }

        [Fact]
        public async Task UpdateUserRole_AsAdmin_ShouldUpdateRole()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            var userIdToUpdate = allUsers.First(u => u.Email == "alice@example.com").UserId;

            var command = new UpdateUserRoleCommand(userIdToUpdate, UserRole.Admin.ToString());

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/admin/users/{userIdToUpdate}/role", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify role updated
            var updatedUserResponse = await _client.GetAsync($"/api/v1/admin/users/{userIdToUpdate}");
            updatedUserResponse.EnsureSuccessStatusCode();
            var updatedUser = await updatedUserResponse.Content.ReadFromJsonAsync<UserDto>();
            Assert.Equal(UserRole.Admin.ToString(), updatedUser.Role);
        }

        [Fact]
        public async Task DeleteUser_AsAdmin_ShouldDeleteUser()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            var userIdToDelete = allUsers.First(u => u.Email == "alice@example.com").UserId;

            // Act
            var response = await _client.DeleteAsync($"/api/v1/admin/users/{userIdToDelete}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify user is deleted
            var deletedUserResponse = await _client.GetAsync($"/api/v1/admin/users/{userIdToDelete}");
            Assert.Equal(HttpStatusCode.NotFound, deletedUserResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_AsAdmin_ShouldUpdateUserData()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            var userIdToUpdate = allUsers.First(u => u.Email == "alice@example.com").UserId;

            var command = new UpdateUserCommand(userIdToUpdate, "alice_updated@example.com", "Alice Updated");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/admin/users/{userIdToUpdate}", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify update
            var updatedUserResponse = await _client.GetAsync($"/api/v1/admin/users/{userIdToUpdate}");
            updatedUserResponse.EnsureSuccessStatusCode();
            var updatedUser = await updatedUserResponse.Content.ReadFromJsonAsync<UserDto>();
            Assert.Equal("alice_updated@example.com", updatedUser.Email);
            Assert.Equal("Alice Updated", updatedUser.Name);
        }

        [Fact]
        public async Task AdminResetPassword_AsAdmin_ShouldResetPassword()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            var userIdToResetPassword = allUsers.First(u => u.Email == "alice@example.com").UserId;

            var command = new AdminResetPasswordCommand(userIdToResetPassword, "NewAdminPassword123!");

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/admin/users/{userIdToResetPassword}/reset-password", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify new password works
            var loginQuery = new LoginUserQuery("alice@example.com", "NewAdminPassword123!");
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
            loginResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ChangeUserPassword_AsAdmin_ShouldChangePassword()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.NotNull(allUsers);
            
            var aliceUser = allUsers.FirstOrDefault(u => u.Email == "alice@example.com");
            Assert.NotNull(aliceUser);
            var userIdToChange = aliceUser.UserId;

            var command = new AdminChangePasswordCommand(userIdToChange, "ChangedPassword123!");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/admin/users/{userIdToChange}/password", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify new password works by logging in with it
            var loginQuery = new LoginUserQuery("alice@example.com", "ChangedPassword123!");
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
            loginResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ChangeUserPassword_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.NotNull(allUsers);
            
            var aliceUser = allUsers.FirstOrDefault(u => u.Email == "alice@example.com");
            Assert.NotNull(aliceUser);
            var correctUserId = aliceUser.UserId;
            var incorrectUserId = Guid.NewGuid(); // Some other random ID

            var command = new AdminChangePasswordCommand(correctUserId, "ChangedPassword123!");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/admin/users/{incorrectUserId}/password", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ChangeUserPassword_AsRegularUser_ShouldReturnForbidden()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsUserAsync("alice@example.com", "password123"); // Log in as regular user

            var allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            Assert.Equal(HttpStatusCode.Forbidden, allUsersResponse.StatusCode);

            // Get admin user ID (assuming we know it from somewhere)
            await AuthenticateAsAdminAsync(); // Temporarily authenticate as admin
            allUsersResponse = await _client.GetAsync("/api/v1/admin/users");
            allUsersResponse.EnsureSuccessStatusCode();
            var allUsers = await allUsersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.NotNull(allUsers);
            
            var adminUser = allUsers.FirstOrDefault(u => u.Role == UserRole.Admin.ToString());
            Assert.NotNull(adminUser);
            var adminId = adminUser.UserId;
            
            // Re-authenticate as regular user
            await AuthenticateAsUserAsync("alice@example.com", "password123");
            
            var command = new AdminChangePasswordCommand(adminId, "ChangedPassword123!");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/admin/users/{adminId}/password", command);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ChangeUserPassword_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            await AuthenticateAsAdminAsync();

            var nonExistentUserId = Guid.NewGuid();
            var command = new AdminChangePasswordCommand(nonExistentUserId, "ChangedPassword123!");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/admin/users/{nonExistentUserId}/password", command);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
