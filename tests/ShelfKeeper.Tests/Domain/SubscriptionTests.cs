// <copyright file="SubscriptionTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;

using ShelfKeeper.Shared.Common;


namespace ShelfKeeper.Tests.Domain
{
    public class SubscriptionTests
    {
        [Fact]
        public void Subscription_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid subscriptionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string plan = "Premium";
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = DateTime.UtcNow.AddYears(1);
            bool autoRenew = true;

            // Act
            Subscription subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = userId,
                Plan = plan,
                StartTime = startTime,
                EndTime = endTime,
                AutoRenew = autoRenew,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(subscriptionId, subscription.Id);
            Assert.Equal(userId, subscription.UserId);
            Assert.Equal(plan, subscription.Plan);
            Assert.Equal(startTime, subscription.StartTime);
            Assert.Equal(endTime, subscription.EndTime);
            Assert.Equal(autoRenew, subscription.AutoRenew);
            Assert.NotEqual(default(DateTime), subscription.CreatedAt);
            Assert.NotEqual(default(DateTime), subscription.LastUpdatedAt);

            OperationResult validationOperationResult = subscription.Validate();
            Assert.True(validationOperationResult.IsSuccess);
        }

        [Fact]
        public void Subscription_ValidationFails_WhenPlanIsEmpty()
        {
            // Arrange
            Subscription subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Plan = string.Empty,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddYears(1),
                AutoRenew = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = subscription.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "Subscription plan cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void Subscription_ValidationFails_WhenEndTimeIsBeforeStartTime()
        {
            // Arrange
            Subscription subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Plan = "Premium",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(-1),
                AutoRenew = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = subscription.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "Subscription end time cannot be before start time." && e.Type == OperationErrorType.ValidationError);
        }
    }
}