// <copyright file="StripeWebhookControllerTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.WebApi.Controllers;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.WebApi.Controllers
{
    public class StripeWebhookControllerTests
    {
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly StripeWebhookController _controller;

        public StripeWebhookControllerTests()
        {
            _mockStripeService = new Mock<IStripeService>();
            _controller = new StripeWebhookController(_mockStripeService.Object);
        }

        [Fact]
        public async Task HandleWebhook_WithValidPayload_ShouldReturnOk()
        {
            // Arrange
            var json = "{\"id\": \"evt_123\", \"type\": \"customer.subscription.created\"}";
            var signature = "test_signature";
            
            // Setup the controller's HttpContext for reading the request body
            SetupHttpContext(_controller, json, signature);
            
            _mockStripeService.Setup(s => s.HandleWebhookEventAsync(json, signature, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            Assert.IsType<OkResult>(result);
            _mockStripeService.Verify(s => s.HandleWebhookEventAsync(json, signature, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleWebhook_WithInvalidPayload_ShouldReturnBadRequest()
        {
            // Arrange
            var json = "{\"id\": \"evt_123\", \"type\": \"unknown.event\"}";
            var signature = "invalid_signature";
            var operationErrors = new List<OperationError>
            {
                new OperationError("Invalid webhook signature.", OperationErrorType.ValidationError)
            };
            
            // Setup the controller's HttpContext for reading the request body
            SetupHttpContext(_controller, json, signature);
            
            _mockStripeService.Setup(s => s.HandleWebhookEventAsync(json, signature, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Failure(operationErrors));

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationErrors, badRequestResult.Value);
            _mockStripeService.Verify(s => s.HandleWebhookEventAsync(json, signature, It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupHttpContext(StripeWebhookController controller, string json, string signature)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var memoryStream = new MemoryStream(bytes);
            memoryStream.Position = 0;
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = memoryStream;
            httpContext.Request.ContentLength = bytes.Length;
            
            httpContext.Request.Headers["Stripe-Signature"] = signature;
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
