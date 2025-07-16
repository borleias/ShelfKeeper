// <copyright file="Subscription.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets or sets the name of the subscription plan (e.g., "Free", "Basic", "Premium").
        /// </summary>
        public SubscriptionPlan Plan { get; set; }

        /// <summary>
        /// Gets or sets the current status of the subscription.
        /// </summary>
        public SubscriptionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Stripe customer ID associated with this subscription.
        /// </summary>
        public string? StripeCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the Stripe subscription ID associated with this subscription.
        /// </summary>
        public string? StripeSubscriptionId { get; set; }

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

        /// <summary>
        /// Validates the subscription entity properties.
        /// </summary>
        /// <returns>A <see cref="OperationResult"/> indicating the success or failure of the validation.</returns>
        public new OperationResult Validate()
        {
            List<OperationError> errors = new List<OperationError>();

            OperationResult baseValidation = base.Validate();
            if (baseValidation.IsFailure)
            {
                errors.AddRange(baseValidation.Errors);
            }

            if (EndTime < StartTime)
            {
                errors.Add(new OperationError("Subscription end time cannot be before start time.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}
