// <copyright file="UsersControllerTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.WebApi.Controllers;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.WebApi.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task Register_WithValidCommand_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var command = new CreateUserCommand("test@example.com", "Password123!", "Test User");
            var responseValue = new CreateUserResponse(Guid.NewGuid(), "test@example.com", "Test User");
            
            _mockUserService.Setup(s => s.CreateUserAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<CreateUserResponse>.Success(responseValue));

            // Act
            var result = await _controller.Register(command);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.Register), createdAtActionResult.ActionName);
            Assert.Equal(responseValue, createdAtActionResult.Value);
            _mockUserService.Verify(s => s.CreateUserAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Register_WithInvalidCommand_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new CreateUserCommand("invalid-email", "Test User", "weak");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Invalid email format.", OperationErrorType.ValidationError)
            };
            
            _mockUserService.Setup(s => s.CreateUserAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<CreateUserResponse>.Failure(operationErrors));

            // Act
            var result = await _controller.Register(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockUserService.Verify(s => s.CreateUserAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOk()
        {
            // Arrange
            var query = new LoginUserQuery("test@example.com", "Password123!");
            var responseValue = new LoginUserResponse(Guid.NewGuid(), "test@example.com", "Test User", "jwt-token");
            
            _mockUserService.Setup(s => s.LoginUserAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<LoginUserResponse>.Success(responseValue));

            // Act
            var result = await _controller.Login(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(responseValue, okResult.Value);
            _mockUserService.Verify(s => s.LoginUserAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var query = new LoginUserQuery("test@example.com", "WrongPassword");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Invalid credentials.", OperationErrorType.UnauthorizedError)
            };
            
            _mockUserService.Setup(s => s.LoginUserAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<LoginUserResponse>.Failure(operationErrors));

            // Act
            var result = await _controller.Login(query);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(operationErrors, unauthorizedResult.Value);
            _mockUserService.Verify(s => s.LoginUserAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithValidationError_ShouldReturnBadRequest()
        {
            // Arrange
            var query = new LoginUserQuery("", "");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Email and password are required.", OperationErrorType.ValidationError)
            };
            
            _mockUserService.Setup(s => s.LoginUserAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<LoginUserResponse>.Failure(operationErrors));

            // Act
            var result = await _controller.Login(query);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockUserService.Verify(s => s.LoginUserAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WithValidRequest_ShouldReturnNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand(userId, "OldPassword123!", "NewPassword123!");
            
            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockUserService.Setup(s => s.ChangePasswordAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.ChangePassword(command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockUserService.Verify(s => s.ChangePasswordAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WithMismatchedUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var command = new ChangePasswordCommand(differentUserId, "OldPassword123!", "NewPassword123!");
            
            // Setup user identity with a different ID than the one in the command
            SetupUserIdentity(_controller, userId);

            // Act
            var result = await _controller.ChangePassword(command);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockUserService.Verify(s => s.ChangePasswordAsync(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangePassword_WithNoUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword123!", "NewPassword123!");
            
            // No user identity setup

            // Act
            var result = await _controller.ChangePassword(command);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockUserService.Verify(s => s.ChangePasswordAsync(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangePassword_WithValidationError_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand(userId, "OldPassword123!", "weak");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Password does not meet complexity requirements.", OperationErrorType.ValidationError)
            };
            
            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockUserService.Setup(s => s.ChangePasswordAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.ChangePassword(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockUserService.Verify(s => s.ChangePasswordAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_WithValidEmail_ShouldReturnNoContent()
        {
            // Arrange
            var command = new ForgotPasswordCommand("test@example.com");
            
            _mockUserService.Setup(s => s.ForgotPasswordAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.ForgotPassword(command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockUserService.Verify(s => s.ForgotPasswordAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordWithToken_WithValidToken_ShouldReturnNoContent()
        {
            // Arrange
            var command = new ResetPasswordWithTokenCommand("test@example.com", "valid-token", "NewPassword123!");
            
            _mockUserService.Setup(s => s.ResetPasswordWithTokenAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.ResetPasswordWithToken(command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockUserService.Verify(s => s.ResetPasswordWithTokenAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordWithToken_WithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new ResetPasswordWithTokenCommand("test@example.com", "invalid-token", "NewPassword123!");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Invalid or expired token.", OperationErrorType.ValidationError)
            };
            
            _mockUserService.Setup(s => s.ResetPasswordWithTokenAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.ResetPasswordWithToken(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockUserService.Verify(s => s.ResetPasswordWithTokenAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WithValidUserId_ShouldReturnNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockUserService.Setup(s => s.DeleteUserAsync(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.Delete();

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockUserService.Verify(s => s.DeleteUserAsync(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WithNoUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            // No user identity setup

            // Act
            var result = await _controller.Delete();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockUserService.Verify(s => s.DeleteUserAsync(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WithUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var operationErrors = new List<OperationError>
            {
                new OperationError("User not found.", OperationErrorType.NotFoundError)
            };
            
            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockUserService.Setup(s => s.DeleteUserAsync(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.Delete();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(operationErrors, notFoundResult.Value);
            _mockUserService.Verify(s => s.DeleteUserAsync(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WithValidationError_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var operationErrors = new List<OperationError>
            {
                new OperationError("Cannot delete user with active subscriptions.", OperationErrorType.ValidationError)
            };
            
            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockUserService.Setup(s => s.DeleteUserAsync(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.Delete();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockUserService.Verify(s => s.DeleteUserAsync(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupUserIdentity(UsersController controller, Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
