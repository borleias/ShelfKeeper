// <copyright file="SubscriptionCheckerSettings.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Application.Services.Subscriptions
{
    /// <summary>
    /// Configuration settings for the subscription checker background service.
    /// </summary>
    public class SubscriptionCheckerSettings
    {
        /// <summary>
        /// Gets or sets the interval in hours for the subscription checker to run.
        /// Default is 24 hours.
        /// </summary>
        public int CheckIntervalHours { get; set; } = 24;
    }
}
