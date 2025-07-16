// <copyright file="FeatureGateActionFilter.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShelfKeeper.Application.Services.FeatureGates;
using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Security.Claims;

namespace ShelfKeeper.WebApi.Filters
{
    /// <summary>
    /// An action filter that checks if the authenticated user has access to a specific feature.
    /// </summary>
    public class FeatureGateActionFilter : IAsyncActionFilter
    {
        private readonly IFeatureGateService _featureGateService;
        private readonly FeatureType _featureType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureGateActionFilter"/> class.
        /// </summary>
        /// <param name="featureGateService">The feature gate service.</param>
        /// <param name="featureType">The type of feature to check access for.</param>
        public FeatureGateActionFilter(IFeatureGateService featureGateService, FeatureType featureType)
        {
            _featureGateService = featureGateService;
            _featureType = featureType;
        }

        /// <summary>
        /// Called asynchronously before the action method is invoked.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutingContext"/>.</param>
        /// <param name="next">The <see cref="ActionExecutionDelegate"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Guid userId = GetUserId(context.HttpContext.User);

            if (userId == Guid.Empty)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            OperationResult hasAccessResult = await _featureGateService.HasAccessAsync(userId, _featureType, CancellationToken.None);

            if (hasAccessResult.IsFailure)
            {
                if (hasAccessResult.Errors.Any(e => e.Type == OperationErrorType.UnauthorizedError))
                {
                    context.Result = new UnauthorizedObjectResult(hasAccessResult.Errors);
                }
                else if (hasAccessResult.Errors.Any(e => e.Type == OperationErrorType.ForbiddenError))
                {
                    context.Result = new ForbidResult();
                }
                else
                {
                    context.Result = new BadRequestObjectResult(hasAccessResult.Errors);
                }
                return;
            }

            await next();
        }

        private Guid GetUserId(ClaimsPrincipal user)
        {
            string? userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}
