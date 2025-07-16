// <copyright file="UserServiceTests.cs" company="ShelfKeeper">
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
using System.Linq;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.Application.Services.Users
{
    public class UserServiceTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();

            _userService = new UserService(
                _mockContext.Object,
                _mockPasswordHasher.Object,
                _mockJwtService.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserAndReturnResponse()
        {
            // Arrange
            var command = new CreateUserCommand("test@example.com", "password123", "Test User");
            var users = new List<User>();

            _mockPasswordHasher.Setup(ph => ph.HashPassword(command.Password)).Returns("hashedpassword");
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var operationResult = await _userService.CreateUserAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.Equal(command.Email, operationResult.Value.Email);
            Assert.Single(users);
        }

        [Fact]
        public async Task LoginUserAsync_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var query = new LoginUserQuery("test@example.com", "password123");
            var user = new User { Id = Guid.NewGuid(), Email = query.Email, PasswordHash = "hashedpassword", Name = "Test User", Role = ShelfKeeper.Domain.Common.UserRole.User };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(query.Password, user.PasswordHash)).Returns(true);
            _mockJwtService.Setup(js => js.GenerateToken(user.Id, user.Email, user.Name, user.Role.ToString())).Returns("jwt_token");

            // Act
            var operationResult = await _userService.LoginUserAsync(query, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.Equal("jwt_token", operationResult.Value.Token);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidCredentials_ShouldChangePassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand(userId, "oldPassword", "newPassword");
            var user = new User { Id = userId, PasswordHash = "hashedOldPassword" };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(command.OldPassword, user.PasswordHash)).Returns(true);
            _mockPasswordHasher.Setup(ph => ph.HashPassword(command.NewPassword)).Returns("hashedNewPassword");

            // Act
            var operationResult = await _userService.ChangePasswordAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.Equal("hashedNewPassword", user.PasswordHash);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidOldPassword_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand(userId, "wrongOldPassword", "newPassword");
            var user = new User { Id = userId, PasswordHash = "hashedOldPassword" };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(command.OldPassword, user.PasswordHash)).Returns(false);

            // Act
            var operationResult = await _userService.ChangePasswordAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "Invalid old password.");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldSetResetTokenAndExpiration()
        {
            // Arrange
            var command = new ForgotPasswordCommand("test@example.com");
            var user = new User { Email = command.Email };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var operationResult = await _userService.ForgotPasswordAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.NotNull(user.PasswordResetToken);
            Assert.NotNull(user.PasswordResetTokenExpiration);
            Assert.True(user.PasswordResetTokenExpiration > DateTime.UtcNow);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordWithTokenAsync_WithValidToken_ShouldResetPassword()
        {
            // Arrange
            var command = new ResetPasswordWithTokenCommand("valid_token", "test@example.com", "newPassword");
            var user = new User 
            { 
                Email = command.Email, 
                PasswordResetToken = "valid_token", 
                PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1) 
            };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockPasswordHasher.Setup(ph => ph.HashPassword(command.NewPassword)).Returns("hashedNewPassword");

            // Act
            var operationResult = await _userService.ResetPasswordWithTokenAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.Equal("hashedNewPassword", user.PasswordHash);
            Assert.Null(user.PasswordResetToken);
            Assert.Null(user.PasswordResetTokenExpiration);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordWithTokenAsync_WithInvalidToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new ResetPasswordWithTokenCommand("invalid_token", "test@example.com", "newPassword");
            var user = new User 
            { 
                Email = command.Email, 
                PasswordResetToken = "valid_token", 
                PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1) 
            };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var operationResult = await _userService.ResetPasswordWithTokenAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "Invalid token.");
        }

        [Fact]
        public async Task ResetPasswordWithTokenAsync_WithExpiredToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new ResetPasswordWithTokenCommand("expired_token", "test@example.com", "newPassword");
            var user = new User 
            { 
                Email = command.Email, 
                PasswordResetToken = "expired_token", 
                PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(-1) // Expired
            };
            var users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // Act
            var operationResult = await _userService.ResetPasswordWithTokenAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "Invalid token.");
        }
    }
}