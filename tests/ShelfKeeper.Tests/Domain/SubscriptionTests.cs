// <copyright file="SubscriptionTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

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
            SubscriptionPlan plan = SubscriptionPlan.Premium;
            SubscriptionStatus status = SubscriptionStatus.Active;
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = DateTime.UtcNow.AddYears(1);
            bool autoRenew = true;
            string? stripeCustomerId = "cus_123";
            string? stripeSubscriptionId = "sub_123";

            // Act
            Subscription subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = userId,
                Plan = plan,
                Status = status,
                StartTime = startTime,
                EndTime = endTime,
                AutoRenew = autoRenew,
                StripeCustomerId = stripeCustomerId,
                StripeSubscriptionId = stripeSubscriptionId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(subscriptionId, subscription.Id);
            Assert.Equal(userId, subscription.UserId);
            Assert.Equal(plan, subscription.Plan);
            Assert.Equal(status, subscription.Status);
            Assert.Equal(startTime, subscription.StartTime);
            Assert.Equal(endTime, subscription.EndTime);
            Assert.Equal(autoRenew, subscription.AutoRenew);
            Assert.Equal(stripeCustomerId, subscription.StripeCustomerId);
            Assert.Equal(stripeSubscriptionId, subscription.StripeSubscriptionId);
            Assert.NotEqual(default(DateTime), subscription.CreatedAt);
            Assert.NotEqual(default(DateTime), subscription.LastUpdatedAt);

            OperationResult validationOperationResult = subscription.Validate();
            Assert.True(validationOperationResult.IsSuccess);
        }

        [Fact]
        public void Subscription_ValidationFails_WhenEndTimeIsBeforeStartTime()
        {
            // Arrange
            Subscription subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Plan = SubscriptionPlan.Premium,
                Status = SubscriptionStatus.Active,
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
