// <copyright file="SubscriptionStatus.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Domain.Common
{
    /// <summary>
    /// Defines the possible statuses of a subscription.
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <summary>
        /// The subscription is active.
        /// </summary>
        Active = 0,

        /// <summary>
        /// The subscription has been cancelled and will expire at the end of the current period.
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// The subscription has expired.
        /// </summary>
        Expired = 2,

        /// <summary>
        /// The subscription is in a trial period.
        /// </summary>
        Trial = 3,

        /// <summary>
        /// The subscription is paused.
        /// </summary>
        Paused = 4,

        /// <summary>
        /// The subscription is incomplete (e.g., payment failed).
        /// </summary>
        Incomplete = 5,
    }
}
