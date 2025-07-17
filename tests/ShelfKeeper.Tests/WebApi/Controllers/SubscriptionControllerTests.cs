// <copyright file="SubscriptionControllerTests.cs" company="ShelfKeeper">
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
using ShelfKeeper.Application.Services.Subscriptions;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.WebApi.Controllers;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Tests.WebApi.Controllers
{
    public class SubscriptionControllerTests
    {
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly SubscriptionController _controller;
        private readonly Guid _userId;

        public SubscriptionControllerTests()
        {
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _controller = new SubscriptionController(_mockSubscriptionService.Object);
            _userId = Guid.NewGuid();

            // Set up default user identity
            SetupUserIdentity(_controller, _userId);
        }

        [Fact]
        public async Task GetMySubscription_WithValidUserId_ShouldReturnOk()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new SubscriptionDto(subscriptionId, _userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), false);

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscription));

            // Act
            var result = await _controller.GetMySubscription();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(subscription, okResult.Value);
            _mockSubscriptionService.Verify(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMySubscription_WithNoSubscription_ShouldReturnNotFound()
        {
            // Arrange
            var operationErrors = new List<OperationError>
            {
                new OperationError("Subscription not found.", OperationErrorType.NotFoundError)
            };

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Failure(operationErrors));

            // Act
            var result = await _controller.GetMySubscription();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(operationErrors, notFoundResult.Value);
            _mockSubscriptionService.Verify(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMySubscription_WithOtherError_ShouldReturnBadRequest()
        {
            // Arrange
            var operationErrors = new List<OperationError>
            {
                new OperationError("Database error.", OperationErrorType.InternalServerError)
            };

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Failure(operationErrors));

            // Act
            var result = await _controller.GetMySubscription();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockSubscriptionService.Verify(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSubscription_WithValidCommand_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddMonths(1);
            var command = new CreateSubscriptionCommand(Guid.Empty, SubscriptionPlan.Premium, startTime, endTime, true);
            var expectedCommand = command with { UserId = _userId };
            var subscriptionId = Guid.NewGuid();
            var subscription = new SubscriptionDto(subscriptionId, _userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, startTime, endTime, true);

            _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(
                    It.Is<CreateSubscriptionCommand>(c => c.UserId == _userId && c.Plan == command.Plan),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscription));

            // Act
            var result = await _controller.CreateSubscription(command);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetMySubscription), createdAtActionResult.ActionName);
            Assert.Equal(subscription, createdAtActionResult.Value);
            _mockSubscriptionService.Verify(s => s.CreateSubscriptionAsync(
                It.Is<CreateSubscriptionCommand>(c => c.UserId == _userId && c.Plan == command.Plan),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSubscription_WithInvalidCommand_ShouldReturnBadRequest()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddMonths(1);
            var command = new CreateSubscriptionCommand(Guid.Empty, SubscriptionPlan.Premium, startTime, endTime, true);
            var operationErrors = new List<OperationError>
            {
                new OperationError("Invalid plan type.", OperationErrorType.ValidationError)
            };

            _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(
                    It.Is<CreateSubscriptionCommand>(c => c.UserId == _userId && c.Plan == command.Plan),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Failure(operationErrors));

            // Act
            var result = await _controller.CreateSubscription(command);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockSubscriptionService.Verify(s => s.CreateSubscriptionAsync(
                It.Is<CreateSubscriptionCommand>(c => c.UserId == _userId && c.Plan == command.Plan),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelSubscription_WithOwnSubscription_ShouldReturnNoContent()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new SubscriptionDto(subscriptionId, _userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), false);

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscription));
                
            _mockSubscriptionService.Setup(s => s.CancelSubscriptionAsync(
                    It.Is<CancelSubscriptionCommand>(c => c.SubscriptionId == subscriptionId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.CancelSubscription(subscriptionId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubscriptionService.Verify(s => s.CancelSubscriptionAsync(
                It.Is<CancelSubscriptionCommand>(c => c.SubscriptionId == subscriptionId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelSubscription_WithDifferentSubscriptionId_ShouldReturnUnauthorized()
        {
            // Arrange
            var userSubscriptionId = Guid.NewGuid();
            var differentSubscriptionId = Guid.NewGuid();
            var subscription = new SubscriptionDto(userSubscriptionId, _userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), false);

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscription));

            // Act
            var result = await _controller.CancelSubscription(differentSubscriptionId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockSubscriptionService.Verify(s => s.CancelSubscriptionAsync(It.IsAny<CancelSubscriptionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelSubscription_WithNoSubscription_ShouldReturnUnauthorized()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var operationErrors = new List<OperationError>
            {
                new OperationError("Subscription not found.", OperationErrorType.NotFoundError)
            };

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Failure(operationErrors));

            // Act
            var result = await _controller.CancelSubscription(subscriptionId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockSubscriptionService.Verify(s => s.CancelSubscriptionAsync(It.IsAny<CancelSubscriptionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpgradeSubscription_WithOwnSubscription_ShouldReturnNoContent()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new SubscriptionDto(subscriptionId, _userId, SubscriptionPlan.Basic, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), false);
            var command = new UpgradeSubscriptionCommand(Guid.Empty, SubscriptionPlan.Premium);

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscription));
                
            _mockSubscriptionService.Setup(s => s.UpgradeSubscriptionAsync(
                    It.Is<UpgradeSubscriptionCommand>(c => c.SubscriptionId == subscriptionId && c.NewPlan == command.NewPlan),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.UpgradeSubscription(subscriptionId, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubscriptionService.Verify(s => s.UpgradeSubscriptionAsync(
                It.Is<UpgradeSubscriptionCommand>(c => c.SubscriptionId == subscriptionId && c.NewPlan == command.NewPlan),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DowngradeSubscription_WithOwnSubscription_ShouldReturnNoContent()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new SubscriptionDto(subscriptionId, _userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), false);
            var command = new DowngradeSubscriptionCommand(Guid.Empty, SubscriptionPlan.Basic);

            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscription));
                
            _mockSubscriptionService.Setup(s => s.DowngradeSubscriptionAsync(
                    It.Is<DowngradeSubscriptionCommand>(c => c.SubscriptionId == subscriptionId && c.NewPlan == command.NewPlan),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.DowngradeSubscription(subscriptionId, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubscriptionService.Verify(s => s.DowngradeSubscriptionAsync(
                It.Is<DowngradeSubscriptionCommand>(c => c.SubscriptionId == subscriptionId && c.NewPlan == command.NewPlan),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupUserIdentity(SubscriptionController controller, Guid userId)
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
