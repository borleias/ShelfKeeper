// <copyright file="Models.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Application.Services.Subscriptions.Models
{
    /// <summary>
    /// Represents a data transfer object for subscription information.
    /// </summary>
    /// <param name="SubscriptionId">The unique identifier of the subscription.</param>
    /// <param name="UserId">The ID of the user who owns the subscription.</param>
    /// <param name="Plan">The subscription plan.</param>
    /// <param name="Status">The status of the subscription.</param>
    /// <param name="StartTime">The start date and time of the subscription.</param>
    /// <param name="EndTime">The end date and time of the subscription.</param>
    /// <param name="AutoRenew">Indicates whether the subscription automatically renews.</param>
    public record SubscriptionDto(
        Guid SubscriptionId,
        Guid UserId,
        SubscriptionPlan Plan,
        SubscriptionStatus Status,
        DateTime StartTime,
        DateTime EndTime,
        bool AutoRenew);

    /// <summary>
    /// Represents a command to create a new subscription.
    /// </summary>
    /// <param name="UserId">The ID of the user for whom to create the subscription.</param>
    /// <param name="Plan">The subscription plan to create.</param>
    /// <param name="StartTime">The start date and time of the subscription.</param>
    /// <param name="EndTime">The end date and time of the subscription.</param>
    /// <param name="AutoRenew">Indicates whether the subscription automatically renews.</param>
    public record CreateSubscriptionCommand(
        Guid UserId,
        SubscriptionPlan Plan,
        DateTime StartTime,
        DateTime EndTime,
        bool AutoRenew);

    /// <summary>
    /// Represents a command to update the status of a subscription.
    /// </summary>
    /// <param name="SubscriptionId">The ID of the subscription to update.</param>
    /// <param name="NewStatus">The new status for the subscription.</param>
    public record UpdateSubscriptionStatusCommand(Guid SubscriptionId, SubscriptionStatus NewStatus);

    /// <summary>
    /// Represents a command to cancel a subscription.
    /// </summary>
    /// <param name="SubscriptionId">The ID of the subscription to cancel.</param>
    public record CancelSubscriptionCommand(Guid SubscriptionId);

    /// <summary>
    /// Represents a command to upgrade a subscription.
    /// </summary>
    /// <param name="SubscriptionId">The ID of the subscription to upgrade.</param>
    /// <param name="NewPlan">The new plan for the subscription.</param>
    public record UpgradeSubscriptionCommand(Guid SubscriptionId, SubscriptionPlan NewPlan);

    /// <summary>
    /// Represents a command to downgrade a subscription.
    /// </summary>
    /// <param name="SubscriptionId">The ID of the subscription to downgrade.</param>
    /// <param name="NewPlan">The new plan for the subscription.</param>
    public record DowngradeSubscriptionCommand(Guid SubscriptionId, SubscriptionPlan NewPlan);

    /// <summary>
    /// Represents a command to create a Stripe Checkout Session.
    /// </summary>
    /// <param name="UserId">The ID of the user initiating the checkout.</param>
    /// <param name="Plan">The subscription plan to purchase.</param>
    /// <param name="SuccessUrl">The URL to redirect to after successful checkout.</param>
    /// <param name="CancelUrl">The URL to redirect to after cancelled checkout.</param>
    public record CreateCheckoutSessionCommand(Guid UserId, SubscriptionPlan Plan, string SuccessUrl, string CancelUrl);

    /// <summary>
    /// Represents the response containing the URL for the Stripe Checkout Session.
    /// </summary>
    /// <param name="CheckoutUrl">The URL to redirect the user to for checkout.</param>
    public record CreateCheckoutSessionResponse(string CheckoutUrl);
}
