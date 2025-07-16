// <copyright file="FeatureGateService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;
using ShelfKeeper.Application.Services.Subscriptions;
using ShelfKeeper.Application.Services.Subscriptions.Models;

namespace ShelfKeeper.Application.Services.FeatureGates
{
    /// <summary>
    /// Provides services for feature gating based on subscription plans.
    /// </summary>
    public class FeatureGateService : IFeatureGateService
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureGateService"/> class.
        /// </summary>
        /// <param name="subscriptionService">The subscription service.</param>
        /// <param name="context">The application database context.</param>
        public FeatureGateService(ISubscriptionService subscriptionService, IApplicationDbContext context)
        {
            _subscriptionService = subscriptionService;
            _context = context;
        }

        /// <summary>
        /// Checks if a user has access to a specific feature based on their subscription plan.
        /// </summary>
        public async Task<OperationResult> HasAccessAsync(Guid userId, FeatureType feature, CancellationToken cancellationToken)
        {
            OperationResult<SubscriptionDto> subscriptionResult = await _subscriptionService.GetUserSubscriptionAsync(userId, cancellationToken);

            if (subscriptionResult.IsFailure)
            {
                // If no active subscription, assume Free plan
                return feature switch
                {
                    FeatureType.MediaItemLimit => await CheckMediaItemLimit(userId, SubscriptionPlan.Free, cancellationToken),
                    FeatureType.SharedLists => CheckSharedLists(SubscriptionPlan.Free),
                    FeatureType.AdvancedSearch => CheckAdvancedSearch(SubscriptionPlan.Free),
                    FeatureType.BatchOperations => CheckBatchOperations(SubscriptionPlan.Free),
                    FeatureType.CsvImportExport => CheckCsvImportExport(SubscriptionPlan.Free),
                    _ => OperationResult.Failure("Unknown feature.", OperationErrorType.ValidationError),
                };
            }

            SubscriptionDto subscription = subscriptionResult.Value;

            return feature switch
            {
                FeatureType.MediaItemLimit => await CheckMediaItemLimit(userId, subscription.Plan, cancellationToken),
                FeatureType.SharedLists => CheckSharedLists(subscription.Plan),
                FeatureType.AdvancedSearch => CheckAdvancedSearch(subscription.Plan),
                FeatureType.BatchOperations => CheckBatchOperations(subscription.Plan),
                FeatureType.CsvImportExport => CheckCsvImportExport(subscription.Plan),
                _ => OperationResult.Failure("Unknown feature.", OperationErrorType.ValidationError),
            };
        }

        private async Task<OperationResult> CheckMediaItemLimit(Guid userId, SubscriptionPlan plan, CancellationToken cancellationToken)
        {
            int maxMediaItems = plan switch
            {
                SubscriptionPlan.Free => 10,
                SubscriptionPlan.Basic => 100,
                SubscriptionPlan.Premium => int.MaxValue, // Unlimited
                _ => 0,
            };

            int currentMediaItems = await _context.MediaItems.CountAsync(mi => mi.UserId == userId, cancellationToken);

            if (currentMediaItems >= maxMediaItems)
            {
                return OperationResult.Failure($"Media item limit ({maxMediaItems}) exceeded for {plan} plan.", OperationErrorType.ForbiddenError);
            }

            return OperationResult.Success();
        }

        private OperationResult CheckSharedLists(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Premium => OperationResult.Success(),
                _ => OperationResult.Failure("Shared lists require Premium plan.", OperationErrorType.ForbiddenError),
            };
        }

        private OperationResult CheckAdvancedSearch(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Basic => OperationResult.Success(),
                SubscriptionPlan.Premium => OperationResult.Success(),
                _ => OperationResult.Failure("Advanced search requires Basic or Premium plan.", OperationErrorType.ForbiddenError),
            };
        }

        private OperationResult CheckBatchOperations(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Premium => OperationResult.Success(),
                _ => OperationResult.Failure("Batch operations require Premium plan.", OperationErrorType.ForbiddenError),
            };
        }

        private OperationResult CheckCsvImportExport(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Premium => OperationResult.Success(),
                _ => OperationResult.Failure("CSV import/export requires Premium plan.", OperationErrorType.ForbiddenError),
            };
        }
    }
}
