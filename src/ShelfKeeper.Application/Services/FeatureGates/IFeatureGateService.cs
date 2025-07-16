// <copyright file="IFeatureGateService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Application.Services.FeatureGates
{
    /// <summary>
    /// Defines the interface for a feature gating service.
    /// </summary>
    public interface IFeatureGateService
    {
        /// <summary>
        /// Checks if a user has access to a specific feature based on their subscription plan.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="feature">The feature to check access for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> HasAccessAsync(Guid userId, FeatureType feature, CancellationToken cancellationToken);
    }
}
