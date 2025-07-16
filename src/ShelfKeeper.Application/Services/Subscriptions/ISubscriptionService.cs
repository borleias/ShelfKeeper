// <copyright file="ISubscriptionService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Application.Services.Subscriptions
{
    /// <summary>
    /// Defines the interface for managing user subscriptions.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Retrieves the current subscription details for a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with the subscription details.</returns>
        Task<OperationResult<SubscriptionDto>> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new subscription for a user asynchronously.
        /// </summary>
        /// <param name="command">The command containing subscription creation details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with the new subscription's details.</returns>
        Task<OperationResult<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the status of an existing subscription asynchronously.
        /// </summary>
        /// <param name="command">The command containing subscription status update details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> UpdateSubscriptionStatusAsync(UpdateSubscriptionStatusCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels a user's subscription asynchronously.
        /// </summary>
        /// <param name="command">The command containing subscription cancellation details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> CancelSubscriptionAsync(CancelSubscriptionCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Upgrades a user's subscription plan asynchronously.
        /// </summary>
        /// <param name="command">The command containing subscription upgrade details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> UpgradeSubscriptionAsync(UpgradeSubscriptionCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Downgrades a user's subscription plan asynchronously.
        /// </summary>
        /// <param name="command">The command containing subscription downgrade details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> DowngradeSubscriptionAsync(DowngradeSubscriptionCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a Stripe Checkout Session for a new subscription asynchronously.
        /// </summary>
        /// <param name="command">The command containing details for the checkout session.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with the checkout session URL.</returns>
        Task<OperationResult<CreateCheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateCheckoutSessionCommand command, CancellationToken cancellationToken);
    }
}
