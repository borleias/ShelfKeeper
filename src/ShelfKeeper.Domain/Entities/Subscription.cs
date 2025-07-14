// <copyright file="Subscription.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents a user's subscription plan.
    /// </summary>
    public class Subscription : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the ID of the user who owns this subscription.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this subscription.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the name of the subscription plan (e.g., "Free", "Plus", "Premium").
        /// </summary>
        public string Plan { get; set; }

        /// <summary>
        /// Gets or sets the start date and time of the subscription.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time of the subscription.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subscription automatically renews.
        /// </summary>
        public bool AutoRenew { get; set; }
    }
}