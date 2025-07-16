// <copyright file="SubscriptionController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using ShelfKeeper.Application.Services.Subscriptions;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Shared.Common;
using System.Security.Claims;

namespace ShelfKeeper.WebApi.Controllers
{
    /// <summary>
    /// API controller for managing user subscriptions.
    /// </summary>
    [ApiController]
    [Authorize]
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionController"/> class.
        /// </summary>
        /// <param name="subscriptionService">The subscription service.</param>
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Retrieves the current user's subscription details.
        /// </summary>
        /// <returns>The current subscription details.</returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetMySubscription()
        {
            Guid userId = GetUserId();
            OperationResult<SubscriptionDto> result = await _subscriptionService.GetUserSubscriptionAsync(userId, CancellationToken.None);
            if (result.IsFailure)
            {
                if (result.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(result.Errors);
                }
                return BadRequest(result.Errors);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new subscription for the current user.
        /// </summary>
        /// <param name="command">The command containing subscription creation details.</param>
        /// <returns>The newly created subscription details.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionCommand command)
        {
            Guid userId = GetUserId();
            CreateSubscriptionCommand createCommand = command with { UserId = userId };
            OperationResult<SubscriptionDto> result = await _subscriptionService.CreateSubscriptionAsync(createCommand, CancellationToken.None);
            if (result.IsFailure)
            {
                return BadRequest(result.Errors);
            }
            return CreatedAtAction(nameof(GetMySubscription), result.Value);
        }

        /// <summary>
        /// Cancels the current user's subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to cancel.</param>
        /// <returns>No content.</returns>
        [HttpPost("{subscriptionId}/cancel")]
        public async Task<IActionResult> CancelSubscription(Guid subscriptionId)
        {
            // Ensure the user owns the subscription before canceling
            Guid userId = GetUserId();
            OperationResult<SubscriptionDto> currentSubscriptionResult = await _subscriptionService.GetUserSubscriptionAsync(userId, CancellationToken.None);
            if (currentSubscriptionResult.IsFailure || currentSubscriptionResult.Value.SubscriptionId != subscriptionId)
            {
                return Unauthorized(); // Or Forbidden, if they try to cancel someone else's subscription
            }

            OperationResult result = await _subscriptionService.CancelSubscriptionAsync(new CancelSubscriptionCommand(subscriptionId), CancellationToken.None);
            if (result.IsFailure)
            {
                return BadRequest(result.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Upgrades the current user's subscription plan.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to upgrade.</param>
        /// <param name="command">The command containing the new plan.</param>
        /// <returns>No content.</returns>
        [HttpPost("{subscriptionId}/upgrade")]
        public async Task<IActionResult> UpgradeSubscription(Guid subscriptionId, [FromBody] UpgradeSubscriptionCommand command)
        {
            // Ensure the user owns the subscription
            Guid userId = GetUserId();
            OperationResult<SubscriptionDto> currentSubscriptionResult = await _subscriptionService.GetUserSubscriptionAsync(userId, CancellationToken.None);
            if (currentSubscriptionResult.IsFailure || currentSubscriptionResult.Value.SubscriptionId != subscriptionId)
            {
                return Unauthorized();
            }

            UpgradeSubscriptionCommand upgradeCommand = command with { SubscriptionId = subscriptionId };
            OperationResult result = await _subscriptionService.UpgradeSubscriptionAsync(upgradeCommand, CancellationToken.None);
            if (result.IsFailure)
            {
                return BadRequest(result.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Downgrades the current user's subscription plan.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to downgrade.</param>
        /// <param name="command">The command containing the new plan.</param>
        /// <returns>No content.</returns>
        [HttpPost("{subscriptionId}/downgrade")]
        public async Task<IActionResult> DowngradeSubscription(Guid subscriptionId, [FromBody] DowngradeSubscriptionCommand command)
        {
            // Ensure the user owns the subscription
            Guid userId = GetUserId();
            OperationResult<SubscriptionDto> currentSubscriptionResult = await _subscriptionService.GetUserSubscriptionAsync(userId, CancellationToken.None);
            if (currentSubscriptionResult.IsFailure || currentSubscriptionResult.Value.SubscriptionId != subscriptionId)
            {
                return Unauthorized();
            }

            DowngradeSubscriptionCommand downgradeCommand = command with { SubscriptionId = subscriptionId };
            OperationResult result = await _subscriptionService.DowngradeSubscriptionAsync(downgradeCommand, CancellationToken.None);
            if (result.IsFailure)
            {
                return BadRequest(result.Errors);
            }
            return NoContent();
        }

        private Guid GetUserId()
        {
            string? userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}
