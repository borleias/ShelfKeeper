// <copyright file="SubscriptionDowngradeChecker.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Domain.Common;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace ShelfKeeper.Application.Services.Subscriptions
{
    /// <summary>
    /// Background service to check for subscription downgrades and media item limits.
    /// </summary>
    public class SubscriptionDowngradeChecker : BackgroundService
    {
        private readonly ILogger<SubscriptionDowngradeChecker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;
        private readonly SubscriptionCheckerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDowngradeChecker"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="scopeFactory">The service scope factory.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="settings">The subscription checker settings.</param>
        public SubscriptionDowngradeChecker(ILogger<SubscriptionDowngradeChecker> logger, IServiceScopeFactory scopeFactory, IEmailService emailService, IOptions<SubscriptionCheckerSettings> settings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _emailService = emailService;
            _settings = settings.Value;
        }

        /// <summary>
        /// Executes the background service logic.
        /// </summary>
        /// <param name="stoppingToken">Triggered when the application host is performing a graceful shutdown.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Downgrade Checker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking for media item limit violations due to subscription downgrades.");

                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    IApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    // Find users whose active subscription plan is Free or Basic
                    List<Subscription> activeSubscriptions = await dbContext.Subscriptions
                        .Where(s => s.Status == SubscriptionStatus.Active && (s.Plan == SubscriptionPlan.Free || s.Plan == SubscriptionPlan.Basic))
                        .Include(s => s.User) // Include user to get email for notifications
                        .ToListAsync(stoppingToken);

                    foreach (Subscription subscription in activeSubscriptions)
                    {
                        int maxMediaItems = subscription.Plan switch
                        {
                            SubscriptionPlan.Free => 10,
                            SubscriptionPlan.Basic => 100,
                            _ => int.MaxValue, // Should not happen for active subscriptions in this query
                        };

                        int currentMediaItems = await dbContext.MediaItems.CountAsync(mi => mi.UserId == subscription.UserId, stoppingToken);

                        if (currentMediaItems > maxMediaItems)
                        {
                            _logger.LogWarning("User {UserId} ({Email}) has exceeded media item limit for {Plan} plan. Current: {Current}, Max: {Max}",
                                subscription.UserId, subscription.User.Email, subscription.Plan, currentMediaItems, maxMediaItems);

                            // Send email notification to the user
                            string subject = "ShelfKeeper: Media Item Limit Exceeded!";
                            string body = $"Dear {subscription.User.Name},\n\nYour current {subscription.Plan} plan allows a maximum of {maxMediaItems} media items. You currently have {currentMediaItems} media items.\n\nPlease upgrade your subscription or remove some items to comply with your plan's limit.\n\nBest regards,\nYour ShelfKeeper Team";

                            await _emailService.SendEmailAsync(subscription.User.Email, subject, body, stoppingToken);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromHours(_settings.CheckIntervalHours), stoppingToken); // Check every configured hours
            }

            _logger.LogInformation("Subscription Downgrade Checker stopped.");
        }
    }
}
