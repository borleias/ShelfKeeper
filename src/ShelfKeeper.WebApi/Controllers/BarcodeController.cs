// <copyright file="BarcodeController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Application.Services.MediaItems;
using ShelfKeeper.Application.Services.MediaItems.Models;
using Microsoft.AspNetCore.Authorization;
using ShelfKeeper.Shared.Common;
using System.Security.Claims;

namespace ShelfKeeper.WebApi.Controllers
{
    /// <summary>
    /// API controller for barcode scanning functionality.
    /// </summary>
    [ApiController]
    [Authorize]
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BarcodeController : ControllerBase
    {
        private readonly IBarcodeScannerService _barcodeScannerService;
        private readonly IMediaItemService _mediaItemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeController"/> class.
        /// </summary>
        /// <param name="barcodeScannerService">The barcode scanner service.</param>
        /// <param name="mediaItemService">The media item service.</param>
        public BarcodeController(IBarcodeScannerService barcodeScannerService, IMediaItemService mediaItemService)
        {
            _barcodeScannerService = barcodeScannerService;
            _mediaItemService = mediaItemService;
        }

        /// <summary>
        /// Scans a barcode and creates a new media item based on the scanned data.
        /// </summary>
        /// <param name="barcode">The barcode to scan.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpGet("{barcode}")]
        public async Task<IActionResult> Scan(string barcode)
        {
            var userId = GetUserId();
            CreateMediaItemCommand command = await _barcodeScannerService.ScanBarcodeAsync(barcode);
            // Assign the actual UserId from the authenticated user
            CreateMediaItemCommand createMediaItemCommand = command with { UserId = userId };

            OperationResult<CreateMediaItemResponse> response = await _mediaItemService.CreateMediaItemAsync(createMediaItemCommand, CancellationToken.None);
            return Ok(response.Value);
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}