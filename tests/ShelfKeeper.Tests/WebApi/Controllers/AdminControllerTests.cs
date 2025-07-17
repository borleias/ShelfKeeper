// <copyright file="AdminControllerTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Domain.Common;
using ShelfKeeper.WebApi.Controllers;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.WebApi.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminUserService> _mockAdminUserService;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockAdminUserService = new Mock<IAdminUserService>();
            _controller = new AdminController(_mockAdminUserService.Object);
        }

        [Fact]
        public async Task ChangeUserPassword_WithValidIdAndMatchingCommand_ShouldReturnNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            _mockAdminUserService.Setup(s => s.ChangeUserPasswordAsAdminAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.ChangeUserPassword(userId, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockAdminUserService.Verify(s => s.ChangeUserPasswordAsAdminAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangeUserPassword_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var commandUserId = Guid.NewGuid();
            var command = new AdminChangePasswordCommand(commandUserId, "newPassword123");

            // Act
            var result = await _controller.ChangeUserPassword(userId, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockAdminUserService.Verify(s => s.ChangeUserPasswordAsAdminAsync(It.IsAny<AdminChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            var operationErrors = new List<OperationError>
            {
                new OperationError("User not found.", OperationErrorType.NotFoundError)
            };
            _mockAdminUserService.Setup(s => s.ChangeUserPasswordAsAdminAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.ChangeUserPassword(userId, command);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal(operationErrors, notFoundResult.Value);
        }

        [Fact]
        public async Task ChangeUserPassword_WithValidationError_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Password does not meet requirements.", OperationErrorType.ValidationError)
            };
            _mockAdminUserService.Setup(s => s.ChangeUserPasswordAsAdminAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.ChangeUserPassword(userId, command);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal(operationErrors, badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeUserPassword_WithNullCommand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            AdminChangePasswordCommand? command = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.ChangeUserPassword(userId, command!));
            _mockAdminUserService.Verify(s => s.ChangeUserPasswordAsAdminAsync(It.IsAny<AdminChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUserPassword_WithEmptyGuidId_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.Empty;
            var command = new AdminChangePasswordCommand(Guid.Empty, "newPassword123");

            // Act
            var result = await _controller.ChangeUserPassword(userId, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockAdminUserService.Verify(s => s.ChangeUserPasswordAsAdminAsync(It.IsAny<AdminChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeUserPassword_WithDatabaseError_ShouldReturnInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AdminChangePasswordCommand(userId, "newPassword123");
            var operationErrors = new List<OperationError>
            {
                new OperationError("Database error occurred.", OperationErrorType.InternalServerError)
            };
            _mockAdminUserService.Setup(s => s.ChangeUserPasswordAsAdminAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.ChangeUserPassword(userId, command);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal(operationErrors, objectResult.Value);
        }
    }
}
