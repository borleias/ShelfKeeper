// <copyright file="SubscriptionService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Application.Services.Subscriptions
{
    /// <summary>
    /// Provides services for managing user subscriptions.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IApplicationDbContext _context;
        private readonly IStripeService _stripeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="stripeService">The Stripe service.</param>
        public SubscriptionService(IApplicationDbContext context, IStripeService stripeService)
        {
            _context = context;
            _stripeService = stripeService;
        }

        /// <summary>
        /// Retrieves the current subscription details for a user asynchronously.
        /// </summary>
        public async Task<OperationResult<SubscriptionDto>> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
        {
            Subscription? subscription = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
            {
                return OperationResult<SubscriptionDto>.Failure("Active subscription not found.", OperationErrorType.NotFoundError);
            }

            return OperationResult<SubscriptionDto>.Success(new SubscriptionDto(
                subscription.Id,
                subscription.UserId,
                subscription.Plan,
                subscription.Status,
                subscription.StartTime,
                subscription.EndTime,
                subscription.AutoRenew));
        }

        /// <summary>
        /// Creates a new subscription for a user asynchronously.
        /// </summary>
        public async Task<OperationResult<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken)
        {
            // Deactivate any existing active subscriptions for the user
            List<Subscription> activeSubscriptions = await _context.Subscriptions
                .Where(s => s.UserId == command.UserId && s.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (Subscription sub in activeSubscriptions)
            {
                sub.Status = SubscriptionStatus.Cancelled; // Or Expired, depending on business logic
                sub.LastUpdatedAt = DateTime.UtcNow;
            }

            Subscription newSubscription = new Subscription
            {
                UserId = command.UserId,
                Plan = command.Plan,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                AutoRenew = command.AutoRenew,
                Status = SubscriptionStatus.Active,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(newSubscription);
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult<SubscriptionDto>.Success(new SubscriptionDto(
                newSubscription.Id,
                newSubscription.UserId,
                newSubscription.Plan,
                newSubscription.Status,
                newSubscription.StartTime,
                newSubscription.EndTime,
                newSubscription.AutoRenew));
        }

        /// <summary>
        /// Updates the status of an existing subscription asynchronously.
        /// </summary>
        public async Task<OperationResult> UpdateSubscriptionStatusAsync(UpdateSubscriptionStatusCommand command, CancellationToken cancellationToken)
        {
            Subscription? subscription = await _context.Subscriptions.FindAsync(new object[] { command.SubscriptionId }, cancellationToken);

            if (subscription == null)
            {
                return OperationResult.Failure("Subscription not found.", OperationErrorType.NotFoundError);
            }

            subscription.Status = command.NewStatus;
            subscription.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Cancels a user's subscription asynchronously.
        /// </summary>
        public async Task<OperationResult> CancelSubscriptionAsync(CancelSubscriptionCommand command, CancellationToken cancellationToken)
        {
            Subscription? subscription = await _context.Subscriptions.FindAsync(new object[] { command.SubscriptionId }, cancellationToken);

            if (subscription == null)
            {
                return OperationResult.Failure("Subscription not found.", OperationErrorType.NotFoundError);
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.EndTime = DateTime.UtcNow; // End immediately or at period end?
            subscription.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Upgrades a user's subscription plan asynchronously.
        /// </summary>
        public async Task<OperationResult> UpgradeSubscriptionAsync(UpgradeSubscriptionCommand command, CancellationToken cancellationToken)
        {
            Subscription? subscription = await _context.Subscriptions.FindAsync(new object[] { command.SubscriptionId }, cancellationToken);

            if (subscription == null)
            {
                return OperationResult.Failure("Subscription not found.", OperationErrorType.NotFoundError);
            }

            if (command.NewPlan <= subscription.Plan)
            {
                return OperationResult.Failure("New plan must be an upgrade.", OperationErrorType.ValidationError);
            }

            subscription.Plan = command.NewPlan;
            subscription.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Downgrades a user's subscription plan asynchronously.
        /// </summary>
        public async Task<OperationResult> DowngradeSubscriptionAsync(DowngradeSubscriptionCommand command, CancellationToken cancellationToken)
        {
            Subscription? subscription = await _context.Subscriptions.FindAsync(new object[] { command.SubscriptionId }, cancellationToken);

            if (subscription == null)
            {
                return OperationResult.Failure("Subscription not found.", OperationErrorType.NotFoundError);
            }

            if (command.NewPlan >= subscription.Plan)
            {
                return OperationResult.Failure("New plan must be a downgrade.", OperationErrorType.ValidationError);
            }

            subscription.Plan = command.NewPlan;
            subscription.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Creates a Stripe Checkout Session for a new subscription asynchronously.
        /// </summary>
        public async Task<OperationResult<CreateCheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateCheckoutSessionCommand command, CancellationToken cancellationToken)
        {
            // In a real application, you would fetch the Stripe Price ID based on the SubscriptionPlan
            // For this example, we'll use a dummy price ID.
            string priceId = "price_123"; 

            OperationResult<string> customerIdResult = await _stripeService.CreateCustomerAsync(command.UserId.ToString(), cancellationToken); // Use UserId as a dummy email for customer creation
            if (customerIdResult.IsFailure)
            {
                return OperationResult<CreateCheckoutSessionResponse>.Failure(customerIdResult.Errors);
            }

            string customerId = customerIdResult.Value;

            // Create a Checkout Session
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                Customer = customerId,
                Mode = "subscription",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                SuccessUrl = command.SuccessUrl,
                CancelUrl = command.CancelUrl,
            };

            var service = new Stripe.Checkout.SessionService();
            try
            {
                Stripe.Checkout.Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);
                return OperationResult<CreateCheckoutSessionResponse>.Success(new CreateCheckoutSessionResponse(session.Url));
            }
            catch (Stripe.StripeException ex)
            {
                return OperationResult<CreateCheckoutSessionResponse>.Failure($"Stripe error creating checkout session: {ex.Message}", OperationErrorType.ExternalServiceError);
            }
        }
    }
}
