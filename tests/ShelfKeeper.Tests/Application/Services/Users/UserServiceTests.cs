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
            CreateUserCommand command = new CreateUserCommand("test@example.com", "password123", "Test User");
            List<User> users = new List<User>();

            _mockPasswordHasher.Setup(ph => ph.HashPassword(command.Password)).Returns("hashedpassword");
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            OperationResult<CreateUserResponse> operationResult = await _userService.CreateUserAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            _mockContext.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(command.Email, operationResult.Value.Email);
            Assert.Equal(command.Name, operationResult.Value.Name);
            Assert.NotEqual(Guid.Empty, operationResult.Value.UserId);
            Assert.Single(users);
            Assert.Equal(operationResult.Value.UserId, users[0].Id);
        }

        [Fact]
        public async Task LoginUserAsync_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            LoginUserQuery query = new LoginUserQuery("test@example.com", "password123");
            User user = new User { Id = Guid.NewGuid(), Email = query.Email, PasswordHash = "hashedpassword", Name = "Test User" };
            List<User> users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(query.Password, user.PasswordHash)).Returns(true);
            _mockJwtService.Setup(js => js.GenerateToken(user.Id, user.Email, user.Name)).Returns("jwt_token");

            // Act
            OperationResult<LoginUserResponse> operationResult = await _userService.LoginUserAsync(query, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.NotNull(operationResult.Value.Token);
            Assert.Equal("jwt_token", operationResult.Value.Token);
        }

        [Fact]
        public async Task LoginUserAsync_WithInvalidCredentials_ShouldReturnFailure()
        {
            // Arrange
            LoginUserQuery query = new LoginUserQuery("test@example.com", "wrongpassword");
            User user = new User { Id = Guid.NewGuid(), Email = query.Email, PasswordHash = "hashedpassword", Name = "Test User" };
            List<User> users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(query.Password, user.PasswordHash)).Returns(false);

            // Act
            OperationResult<LoginUserResponse> operationResult = await _userService.LoginUserAsync(query, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "Invalid credentials." && e.Type == OperationErrorType.UnauthorizedError);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldHashNewPasswordAndSave()
        {
            // Arrange
            ResetPasswordCommand command = new ResetPasswordCommand("test@example.com", "newpassword123");
            User user = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = "oldhashedpassword", Name = "Test User", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            List<User> users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockPasswordHasher.Setup(ph => ph.HashPassword(command.NewPassword)).Returns("newhashedpassword");
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            OperationResult operationResult = await _userService.ResetPasswordAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal("newhashedpassword", user.PasswordHash);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveUserAndSave()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            DeleteUserCommand command = new DeleteUserCommand(userId);
            User user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashedpassword", Name = "Test User", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            List<User> users = new List<User> { user };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            OperationResult operationResult = await _userService.DeleteUserAsync(command, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            _mockContext.Verify(c => c.Users.Remove(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Empty(users);
        }
    }
}