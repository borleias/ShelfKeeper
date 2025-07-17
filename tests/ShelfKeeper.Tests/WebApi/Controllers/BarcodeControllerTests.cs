// <copyright file="BarcodeControllerTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Application.Services.MediaItems;
using ShelfKeeper.Application.Services.MediaItems.Models;
using ShelfKeeper.WebApi.Controllers;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.WebApi.Controllers
{
    public class BarcodeControllerTests
    {
        private readonly Mock<IBarcodeScannerService> _mockBarcodeScannerService;
        private readonly Mock<IMediaItemService> _mockMediaItemService;
        private readonly BarcodeController _controller;

        public BarcodeControllerTests()
        {
            _mockBarcodeScannerService = new Mock<IBarcodeScannerService>();
            _mockMediaItemService = new Mock<IMediaItemService>();
            _controller = new BarcodeController(_mockBarcodeScannerService.Object, _mockMediaItemService.Object);
        }

        [Fact]
        public async Task Scan_WithValidBarcode_ShouldReturnOkResult()
        {
            // Arrange
            var barcode = "9781234567890";
            var userId = Guid.NewGuid();
            var command = new CreateMediaItemCommand(
                Guid.Empty, 
                "Test Book", 
                "Book", 
                2020, 
                "9781234567890", 
                "Description", 
                null, 
                null, 
                null);
            var expectedCommand = command with { UserId = userId };
            var response = new CreateMediaItemResponse(Guid.NewGuid(), "Test Book", "Book", "9781234567890");

            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockBarcodeScannerService.Setup(s => s.ScanBarcodeAsync(barcode))
                .ReturnsAsync(command);
                
            _mockMediaItemService.Setup(s => s.CreateMediaItemAsync(
                    It.Is<CreateMediaItemCommand>(c => c.UserId == userId && c.Title == command.Title), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<CreateMediaItemResponse>.Success(response));

            // Act
            var result = await _controller.Scan(barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            _mockBarcodeScannerService.Verify(s => s.ScanBarcodeAsync(barcode), Times.Once);
            _mockMediaItemService.Verify(s => s.CreateMediaItemAsync(
                It.Is<CreateMediaItemCommand>(c => c.UserId == userId && c.Title == command.Title),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Scan_WithNoAuthenticatedUser_ShouldStillWork()
        {
            // Arrange
            var barcode = "9781234567890";
            var command = new CreateMediaItemCommand(
                Guid.Empty, 
                "Test Book", 
                "Book", 
                2020, 
                "9781234567890", 
                "Description", 
                null, 
                null, 
                null);
            var response = new CreateMediaItemResponse(Guid.NewGuid(), "Test Book", "Book", "9781234567890");
            
            // No user identity setup - this will result in Guid.Empty for userId
            
            _mockBarcodeScannerService.Setup(s => s.ScanBarcodeAsync(barcode))
                .ReturnsAsync(command);
                
            _mockMediaItemService.Setup(s => s.CreateMediaItemAsync(
                    It.Is<CreateMediaItemCommand>(c => c.UserId == Guid.Empty && c.Title == command.Title), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<CreateMediaItemResponse>.Success(response));

            // Act
            var result = await _controller.Scan(barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            _mockBarcodeScannerService.Verify(s => s.ScanBarcodeAsync(barcode), Times.Once);
            _mockMediaItemService.Verify(s => s.CreateMediaItemAsync(
                It.Is<CreateMediaItemCommand>(c => c.UserId == Guid.Empty && c.Title == command.Title),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Scan_WithInvalidBarcode_ShouldStillCreateMediaItem()
        {
            // Arrange
            var barcode = "invalid-barcode";
            var userId = Guid.NewGuid();
            // Even with invalid barcode, the service should return a default command
            var command = new CreateMediaItemCommand(
                Guid.Empty, 
                "Unknown Item", 
                "Unknown", 
                null, 
                "invalid-barcode", 
                "No description available", 
                null, 
                null, 
                null);
            var expectedCommand = command with { UserId = userId };
            var response = new CreateMediaItemResponse(Guid.NewGuid(), "Unknown Item", "Unknown", "invalid-barcode");

            // Setup user identity
            SetupUserIdentity(_controller, userId);
            
            _mockBarcodeScannerService.Setup(s => s.ScanBarcodeAsync(barcode))
                .ReturnsAsync(command);
                
            _mockMediaItemService.Setup(s => s.CreateMediaItemAsync(
                    It.Is<CreateMediaItemCommand>(c => c.UserId == userId && c.Title == command.Title), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<CreateMediaItemResponse>.Success(response));

            // Act
            var result = await _controller.Scan(barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            _mockBarcodeScannerService.Verify(s => s.ScanBarcodeAsync(barcode), Times.Once);
            _mockMediaItemService.Verify(s => s.CreateMediaItemAsync(
                It.Is<CreateMediaItemCommand>(c => c.UserId == userId && c.Title == command.Title),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupUserIdentity(BarcodeController controller, Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
