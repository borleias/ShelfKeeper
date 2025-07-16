// <copyright file="StripeWebhookController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Stripe;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.WebApi.Controllers
{
    /// <summary>
    /// API controller for handling Stripe webhooks.
    /// </summary>
    [ApiController]
    [Route("api/v1/stripe/webhooks")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StripeWebhookController"/> class.
        /// </summary>
        /// <param name="stripeService">The Stripe service.</param>
        public StripeWebhookController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        /// <summary>
        /// Receives and processes Stripe webhook events.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string stripeSignature = Request.Headers["Stripe-Signature"];

            OperationResult result = await _stripeService.HandleWebhookEventAsync(json, stripeSignature, CancellationToken.None);

            if (result.IsFailure)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
    }
}
