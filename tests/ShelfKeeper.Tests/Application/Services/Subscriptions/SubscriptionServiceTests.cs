// <copyright file="SubscriptionServiceTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Application.Services.Subscriptions;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Domain.Entities;
using Moq.EntityFrameworkCore;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Tests.Application.Services.Subscriptions
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockStripeService = new Mock<IStripeService>();
            _subscriptionService = new SubscriptionService(_mockContext.Object, _mockStripeService.Object);
        }

        [Fact]
        public async Task GetUserSubscriptionAsync_ShouldReturnActiveSubscription()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptions = new List<Subscription>
            {
                new Subscription { Id = Guid.NewGuid(), UserId = userId, Plan = SubscriptionPlan.Free, Status = SubscriptionStatus.Active, StartTime = DateTime.UtcNow.AddDays(-30), EndTime = DateTime.UtcNow.AddDays(30), AutoRenew = true },
                new Subscription { Id = Guid.NewGuid(), UserId = userId, Plan = SubscriptionPlan.Basic, Status = SubscriptionStatus.Cancelled, StartTime = DateTime.UtcNow.AddDays(-60), EndTime = DateTime.UtcNow.AddDays(-1), AutoRenew = false }
            };
            _mockContext.Setup(c => c.Subscriptions).ReturnsDbSet(subscriptions);

            // Act
            OperationResult<SubscriptionDto> result = await _subscriptionService.GetUserSubscriptionAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(SubscriptionPlan.Free, result.Value.Plan);
            Assert.Equal(SubscriptionStatus.Active, result.Value.Status);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldCreateNewSubscriptionAndDeactivateOldOnes()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var existingSubscription = new Subscription { Id = Guid.NewGuid(), UserId = userId, Plan = SubscriptionPlan.Free, Status = SubscriptionStatus.Active, StartTime = DateTime.UtcNow.AddDays(-30), EndTime = DateTime.UtcNow.AddDays(30), AutoRenew = true };
            var subscriptions = new List<Subscription> { existingSubscription };
            _mockContext.Setup(c => c.Subscriptions).ReturnsDbSet(subscriptions);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new CreateSubscriptionCommand(userId, SubscriptionPlan.Basic, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), true);

            // Act
            OperationResult<SubscriptionDto> result = await _subscriptionService.CreateSubscriptionAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(SubscriptionPlan.Basic, result.Value.Plan);
            Assert.Equal(SubscriptionStatus.Active, result.Value.Status);
            Assert.Equal(SubscriptionStatus.Cancelled, existingSubscription.Status); // Old subscription should be cancelled
            _mockContext.Verify(c => c.Subscriptions.Add(It.IsAny<Subscription>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // One for old, one for new
        }

        [Fact]
        public async Task UpdateSubscriptionStatusAsync_ShouldUpdateStatus()
        {
            // Arrange
            Guid subscriptionId = Guid.NewGuid();
            var subscription = new Subscription { Id = subscriptionId, UserId = Guid.NewGuid(), Plan = SubscriptionPlan.Free, Status = SubscriptionStatus.Active, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddYears(1), AutoRenew = true };
            var subscriptions = new List<Subscription> { subscription };
            _mockContext.Setup(c => c.Subscriptions).ReturnsDbSet(subscriptions);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new UpdateSubscriptionStatusCommand(subscriptionId, SubscriptionStatus.Cancelled);

            // Act
            OperationResult result = await _subscriptionService.UpdateSubscriptionStatusAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ShouldSetStatusToCancelled()
        {
            // Arrange
            Guid subscriptionId = Guid.NewGuid();
            var subscription = new Subscription { Id = subscriptionId, UserId = Guid.NewGuid(), Plan = SubscriptionPlan.Basic, Status = SubscriptionStatus.Active, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddYears(1), AutoRenew = true };
            var subscriptions = new List<Subscription> { subscription };
            _mockContext.Setup(c => c.Subscriptions).ReturnsDbSet(subscriptions);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new CancelSubscriptionCommand(subscriptionId);

            // Act
            OperationResult result = await _subscriptionService.CancelSubscriptionAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
            Assert.True(subscription.EndTime <= DateTime.UtcNow); // EndTime should be updated
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpgradeSubscriptionAsync_ShouldChangePlan()
        {
            // Arrange
            Guid subscriptionId = Guid.NewGuid();
            var subscription = new Subscription { Id = subscriptionId, UserId = Guid.NewGuid(), Plan = SubscriptionPlan.Free, Status = SubscriptionStatus.Active, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddYears(1), AutoRenew = true };
            var subscriptions = new List<Subscription> { subscription };
            _mockContext.Setup(c => c.Subscriptions).ReturnsDbSet(subscriptions);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new UpgradeSubscriptionCommand(subscriptionId, SubscriptionPlan.Basic);

            // Act
            OperationResult result = await _subscriptionService.UpgradeSubscriptionAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SubscriptionPlan.Basic, subscription.Plan);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DowngradeSubscriptionAsync_ShouldChangePlan()
        {
            // Arrange
            Guid subscriptionId = Guid.NewGuid();
            var subscription = new Subscription { Id = subscriptionId, UserId = Guid.NewGuid(), Plan = SubscriptionPlan.Premium, Status = SubscriptionStatus.Active, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddYears(1), AutoRenew = true };
            var subscriptions = new List<Subscription> { subscription };
            _mockContext.Setup(c => c.Subscriptions).ReturnsDbSet(subscriptions);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new DowngradeSubscriptionCommand(subscriptionId, SubscriptionPlan.Basic);

            // Act
            OperationResult result = await _subscriptionService.DowngradeSubscriptionAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(SubscriptionPlan.Basic, subscription.Plan);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
