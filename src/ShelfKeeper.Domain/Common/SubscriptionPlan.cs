// <copyright file="SubscriptionPlan.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Domain.Common
{
    /// <summary>
    /// Defines the different subscription plans available in the application.
    /// </summary>
    public enum SubscriptionPlan
    {
        /// <summary>
        /// Free subscription plan with limited features.
        /// </summary>
        Free = 0,

        /// <summary>
        /// Basic subscription plan with more features than Free.
        /// </summary>
        Basic = 1,

        /// <summary>
        /// Premium subscription plan with all features.
        /// </summary>
        Premium = 2,
    }
}
