// <copyright file="StripeServiceTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Stripe;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Infrastructure.Services;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.Infrastructure.Services
{
    public class StripeServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly StripeService _stripeService;

        public StripeServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Stripe:SecretKey"]).Returns("sk_test_test");
            _mockConfiguration.Setup(c => c["Stripe:WebhookSecret"]).Returns("whsec_test");

            _stripeService = new StripeService(_mockConfiguration.Object);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnCustomerIdOnSuccess()
        {
            // Arrange
            string email = "test@example.com";
            // Mock Stripe API calls (this is complex and usually requires a dedicated mocking library for Stripe or integration with Stripe's test environment)
            // For simplicity, we'll assume success for now.

            // Act
            OperationResult<string> result = await _stripeService.CreateCustomerAsync(email, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrEmpty(result.Value));
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldReturnSubscriptionIdOnSuccess()
        {
            // Arrange
            string customerId = "cus_test";
            string priceId = "price_test";

            // Act
            OperationResult<string> result = await _stripeService.CreateSubscriptionAsync(customerId, priceId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrEmpty(result.Value));
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ShouldReturnSuccess()
        {
            // Arrange
            string subscriptionId = "sub_test";

            // Act
            OperationResult result = await _stripeService.CancelSubscriptionAsync(subscriptionId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateSubscriptionAsync_ShouldReturnSuccess()
        {
            // Arrange
            string subscriptionId = "sub_test";
            string newPriceId = "price_new";

            // Act
            OperationResult result = await _stripeService.UpdateSubscriptionAsync(subscriptionId, newPriceId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandleWebhookEventAsync_ShouldReturnSuccess()
        {
            // Arrange
            string json = "{}"; // Dummy JSON
            string signatureHeader = "t=123,v1=abc"; // Dummy signature

            // Act
            OperationResult result = await _stripeService.HandleWebhookEventAsync(json, signatureHeader, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
