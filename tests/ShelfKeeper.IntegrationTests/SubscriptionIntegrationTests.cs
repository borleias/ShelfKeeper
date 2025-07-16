// <copyright file="SubscriptionIntegrationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net.Http.Json;
using System.Net;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ShelfKeeper.IntegrationTests
{
    public class SubscriptionIntegrationTests : IntegrationTestBase
    {
        public SubscriptionIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetMySubscription_ShouldReturnSubscriptionDetails()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("subuser@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Create a subscription for the user directly in DB for test setup
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                dbContext.Subscriptions.Add(new ShelfKeeper.Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Plan = SubscriptionPlan.Basic,
                    Status = SubscriptionStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddYears(1),
                    AutoRenew = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                });
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("/api/v1/subscription/me");

            // Assert
            response.EnsureSuccessStatusCode();
            var subscription = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
            Assert.NotNull(subscription);
            Assert.Equal(user.UserId, subscription.UserId);
            Assert.Equal(SubscriptionPlan.Basic, subscription.Plan);
        }

        [Fact]
        public async Task CreateSubscription_ShouldReturnNewSubscription()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("newsub@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var command = new CreateSubscriptionCommand
            (
                UserId: user.UserId,
                Plan: SubscriptionPlan.Premium,
                StartTime: DateTime.UtcNow,
                EndTime: DateTime.UtcNow.AddYears(1),
                AutoRenew: true
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/subscription", command);

            // Assert
            response.EnsureSuccessStatusCode();
            var subscription = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
            Assert.NotNull(subscription);
            Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        }

        [Fact]
        public async Task CancelSubscription_ShouldSetStatusToCancelled()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("cancelsub@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Create a subscription for the user directly in DB for test setup
            Guid subscriptionId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                var sub = new ShelfKeeper.Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Plan = SubscriptionPlan.Basic,
                    Status = SubscriptionStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddYears(1),
                    AutoRenew = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };
                dbContext.Subscriptions.Add(sub);
                await dbContext.SaveChangesAsync();
                subscriptionId = sub.Id;
            }

            // Act
            var response = await _client.PostAsync($"/api/v1/subscription/{subscriptionId}/cancel", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify status in DB
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                var sub = await dbContext.Subscriptions.FindAsync(subscriptionId);
                Assert.NotNull(sub);
                Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
            }
        }

        [Fact]
        public async Task UpgradeSubscription_ShouldChangePlan()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("upgradesub@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Create a subscription for the user directly in DB for test setup
            Guid subscriptionId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                var sub = new ShelfKeeper.Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Plan = SubscriptionPlan.Free,
                    Status = SubscriptionStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddYears(1),
                    AutoRenew = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };
                dbContext.Subscriptions.Add(sub);
                await dbContext.SaveChangesAsync();
                subscriptionId = sub.Id;
            }

            var command = new UpgradeSubscriptionCommand(subscriptionId, SubscriptionPlan.Basic);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/subscription/{subscriptionId}/upgrade", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify plan in DB
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                var sub = await dbContext.Subscriptions.FindAsync(subscriptionId);
                Assert.NotNull(sub);
                Assert.Equal(SubscriptionPlan.Basic, sub.Plan);
            }
        }

        [Fact]
        public async Task DowngradeSubscription_ShouldChangePlan()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("downgradesub@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Create a subscription for the user directly in DB for test setup
            Guid subscriptionId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                var sub = new ShelfKeeper.Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Plan = SubscriptionPlan.Premium,
                    Status = SubscriptionStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddYears(1),
                    AutoRenew = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };
                dbContext.Subscriptions.Add(sub);
                await dbContext.SaveChangesAsync();
                subscriptionId = sub.Id;
            }

            var command = new DowngradeSubscriptionCommand(subscriptionId, SubscriptionPlan.Basic);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/subscription/{subscriptionId}/downgrade", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify plan in DB
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ShelfKeeper.Infrastructure.Persistence.ApplicationDbContext>();
                var sub = await dbContext.Subscriptions.FindAsync(subscriptionId);
                Assert.NotNull(sub);
                Assert.Equal(SubscriptionPlan.Basic, sub.Plan);
            }
        }

        private async Task<LoginUserResponse> RegisterAndLoginUser(string email, string password)
        {
            var registerCommand = new CreateUserCommand(email, password, "Test User");
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);
            registerResponse.EnsureSuccessStatusCode();

            var loginQuery = new LoginUserQuery(email, password);
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
            loginResponse.EnsureSuccessStatusCode();
            return await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        }

        [Fact]
        public async Task HandleWebhookEvent_ShouldReturnOk()
        {
            // Arrange
            await ResetDatabaseAsync();
            // Mock the StripeService to return success for any webhook event
            _factory.MockStripeService
                .Setup(s => s.HandleWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            var jsonPayload = "{}"; // Dummy JSON payload
            var signatureHeader = "t=123,v1=abc"; // Dummy signature header

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/stripe/webhooks");
            request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            request.Headers.Add("Stripe-Signature", signatureHeader);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _factory.MockStripeService.Verify(s => s.HandleWebhookEventAsync(jsonPayload, signatureHeader, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
