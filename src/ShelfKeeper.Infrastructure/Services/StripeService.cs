// <copyright file="StripeService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Stripe;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Shared.Common;
using Microsoft.Extensions.Configuration;

namespace ShelfKeeper.Infrastructure.Services
{
    // ...

    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<OperationResult<string>> CreateCustomerAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                var options = new CustomerCreateOptions
                {
                    Email = email,
                };
                var service = new CustomerService();
                Customer customer = await service.CreateAsync(options, cancellationToken: cancellationToken);
                return OperationResult<string>.Success(customer.Id);
            }
            catch (StripeException ex)
            {
                return OperationResult<string>.Failure($"Stripe error: {ex.Message}", OperationErrorType.ExternalServiceError);
            }
        }

        public async Task<OperationResult<string>> CreateSubscriptionAsync(string customerId, string priceId, CancellationToken cancellationToken)
        {
            try
            {
                var options = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = priceId,
                        },
                    },
                };
                var service = new SubscriptionService();
                Subscription subscription = await service.CreateAsync(options, cancellationToken: cancellationToken);
                return OperationResult<string>.Success(subscription.Id);
            }
            catch (StripeException ex)
            {
                return OperationResult<string>.Failure($"Stripe error: {ex.Message}", OperationErrorType.ExternalServiceError);
            }
        }

        public async Task<OperationResult> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
        {
            try
            {
                var service = new SubscriptionService();
                await service.CancelAsync(subscriptionId, cancellationToken: cancellationToken);
                return OperationResult.Success();
            }
            catch (StripeException ex)
            {
                return OperationResult.Failure($"Stripe error: {ex.Message}", OperationErrorType.ExternalServiceError);
            }
        }

        public async Task<OperationResult> UpdateSubscriptionAsync(string subscriptionId, string newPriceId, CancellationToken cancellationToken)
        {
            try
            {
                var options = new SubscriptionUpdateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Id = subscriptionId,
                            Price = newPriceId,
                        },
                    },
                };
                var service = new SubscriptionService();
                await service.UpdateAsync(subscriptionId, options, cancellationToken: cancellationToken);
                return OperationResult.Success();
            }
            catch (StripeException ex)
            {
                return OperationResult.Failure($"Stripe error: {ex.Message}", OperationErrorType.ExternalServiceError);
            }
        }

        public async Task<OperationResult> HandleWebhookEventAsync(string json, string signatureHeader, CancellationToken cancellationToken)
        {
            string webhookSecret = _configuration["Stripe:WebhookSecret"];
            try
            {
                Event stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

                switch (stripeEvent.Type)
                {
                    case "customer.subscription.created": // Use string literals for event types
                        break;
                    case "customer.subscription.updated":
                        break;
                    case "customer.subscription.deleted":
                        break;
                    case "invoice.payment_succeeded":
                        break;
                    default:
                        break;
                }

                return OperationResult.Success();
            }
            catch (StripeException ex)
            {
                return OperationResult.Failure($"Stripe webhook error: {ex.Message}", OperationErrorType.ExternalServiceError);
            }
        }
    }
}

