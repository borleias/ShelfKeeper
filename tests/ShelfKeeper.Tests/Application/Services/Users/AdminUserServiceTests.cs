// <copyright file="AdminUserServiceTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Domain.Entities;
using Moq.EntityFrameworkCore;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Tests.Application.Services.Users
{
    public class AdminUserServiceTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly AdminUserService _adminUserService;

        public AdminUserServiceTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _adminUserService = new AdminUserService(_mockContext.Object, _mockPasswordHasher.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "user1@example.com", Name = "User One", Role = UserRole.User },
                new User { Id = Guid.NewGuid(), Email = "admin1@example.com", Name = "Admin One", Role = UserRole.Admin }
            };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.GetAllUsersAsync(CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);
            Assert.Contains(result.Value, u => u.Email == "user1@example.com");
            Assert.Contains(result.Value, u => u.Email == "admin1@example.com");
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.GetUserByIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(userId, result.Value.UserId);
            Assert.Equal("test@example.com", result.Value.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var users = new List<User>();
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.GetUserByIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.NotFoundError);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldUpdateUserRole()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _adminUserService.UpdateUserRoleAsync(userId, UserRole.Admin, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(UserRole.Admin, user.Role);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_WithInvalidUser_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var users = new List<User>();
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.UpdateUserRoleAsync(userId, UserRole.Admin, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.NotFoundError);
        }

        [Fact]
        public async Task DeleteUserAsAdminAsync_ShouldDeleteUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _adminUserService.DeleteUserAsAdminAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(users);
            _mockContext.Verify(c => c.Users.Remove(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsAdminAsync_WithInvalidUser_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var users = new List<User>();
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.DeleteUserAsAdminAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.NotFoundError);
        }

        [Fact]
        public async Task UpdateUserAsAdminAsync_ShouldUpdateUserData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserCommand(userId, "updated@example.com", "Updated Name");
            var user = new User { Id = userId, Email = "old@example.com", Name = "Old Name" };
            var users = new List<User> { user };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _adminUserService.UpdateUserAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("updated@example.com", user.Email);
            Assert.Equal("Updated Name", user.Name);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AdminResetPasswordAsync_ShouldResetPassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AdminResetPasswordCommand(userId, "newAdminPassword");
            var user = new User { Id = userId, PasswordHash = "oldHashedPassword" };
            var users = new List<User> { user };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockPasswordHasher.Setup(ph => ph.HashPassword(command.NewPassword)).Returns("newHashedPassword");
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _adminUserService.AdminResetPasswordAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("newHashedPassword", user.PasswordHash);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangeUserPasswordAsAdminAsync_WithValidId_ShouldChangePassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockPasswordHasher.Setup(p => p.HashPassword("newPassword123")).Returns("hashedPassword123");

            // Act
            var result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("hashedPassword123", user.PasswordHash);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangeUserPasswordAsAdminAsync_WithInvalidId_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var users = new List<User>();
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.NotFoundError);
            Assert.Contains(result.Errors, e => e.Message == "User not found.");
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUserPasswordAsAdminAsync_ShouldUpdateLastUpdatedAt()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var initialDate = DateTime.UtcNow.AddDays(-1);
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User, LastUpdatedAt = initialDate };
            var users = new List<User> { user };
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockPasswordHasher.Setup(p => p.HashPassword("newPassword123")).Returns("hashedPassword123");

            // Act
            var result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEqual(initialDate, user.LastUpdatedAt);
            Assert.True(user.LastUpdatedAt > initialDate);
        }

        [Fact]
        public async Task ChangeUserPasswordAsAdminAsync_ShouldCallPasswordHasher()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockPasswordHasher.Setup(p => p.HashPassword("newPassword123")).Returns("hashedPassword123").Verifiable();

            // Act
            var result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockPasswordHasher.Verify(p => p.HashPassword("newPassword123"), Times.Once);
        }

        [Fact]
        public async Task ChangeUserPasswordAsAdminAsync_WithDatabaseError_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateException("Database error", new Exception()));
            _mockPasswordHasher.Setup(p => p.HashPassword("newPassword123")).Returns("hashedPassword123");

            // Act
            var result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.InternalServerError);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ChangeUserPasswordAsAdminAsync_WithInvalidPassword_ShouldReturnValidationFailure(string password)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User", Role = UserRole.User };
            var users = new List<User> { user };
            var command = new AdminChangePasswordCommand(userId, password);
            
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.ValidationError);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
