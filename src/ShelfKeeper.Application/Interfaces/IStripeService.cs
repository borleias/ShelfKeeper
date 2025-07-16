// <copyright file="IStripeService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Application.Interfaces
{
    /// <summary>
    /// Defines the interface for interacting with the Stripe API.
    /// </summary>
    public interface IStripeService
    {
        /// <summary>
        /// Creates a new Stripe customer asynchronously.
        /// </summary>
        /// <param name="email">The email address of the customer.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with the Stripe customer ID.</returns>
        Task<OperationResult<string>> CreateCustomerAsync(string email, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new Stripe subscription asynchronously.
        /// </summary>
        /// <param name="customerId">The ID of the Stripe customer.</param>
        /// <param name="priceId">The ID of the Stripe price (plan).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with the Stripe subscription ID.</returns>
        Task<OperationResult<string>> CreateSubscriptionAsync(string customerId, string priceId, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels a Stripe subscription asynchronously.
        /// </summary>
        /// <param name="subscriptionId">The ID of the Stripe subscription to cancel.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a Stripe subscription asynchronously.
        /// </summary>
        /// <param name="subscriptionId">The ID of the Stripe subscription to update.</param>
        /// <param name="newPriceId">The ID of the new Stripe price (plan).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> UpdateSubscriptionAsync(string subscriptionId, string newPriceId, CancellationToken cancellationToken);

        /// <summary>
        /// Handles a Stripe webhook event asynchronously.
        /// </summary>
        /// <param name="json">The raw JSON payload of the webhook event.</param>
        /// <param name="signatureHeader">The Stripe-Signature header from the webhook request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> HandleWebhookEventAsync(string json, string signatureHeader, CancellationToken cancellationToken);
    }
}
