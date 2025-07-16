// <copyright file="MediaItemsController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ShelfKeeper.Application.Services.MediaItems;
using ShelfKeeper.Application.Services.MediaItems.Models;
using Microsoft.AspNetCore.Authorization;
using ShelfKeeper.Shared.Common;
using System.Security.Claims;

namespace ShelfKeeper.WebApi.Controllers
{
    /// <summary>
    /// API controller for managing media items.
    /// </summary>
    [ApiController]
    [Authorize]
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MediaItemsController : ControllerBase
    {
        private readonly IMediaItemService _mediaItemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaItemsController"/> class.
        /// </summary>
        /// <param name="mediaItemService">The media item service.</param>
        public MediaItemsController(IMediaItemService mediaItemService)
        {
            _mediaItemService = mediaItemService;
        }

        /// <summary>
        /// Creates a new media item.
        /// </summary>
        /// <param name="command">The command containing the media item details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMediaItemCommand command)
        {
            var userId = GetUserId();
            var createCommand = command with { UserId = userId };
            OperationResult<CreateMediaItemResponse> response = await _mediaItemService.CreateMediaItemAsync(createCommand, CancellationToken.None);
            return CreatedAtAction(nameof(GetById), new { id = response.Value.MediaItemId }, response.Value);
        }

        /// <summary>
        /// Retrieves a media item by its ID.
        /// </summary>
        /// <param name="id">The ID of the media item.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetUserId();
            OperationResult<GetMediaItemByIdResponse> response = await _mediaItemService.GetMediaItemByIdAsync(new GetMediaItemByIdQuery(id, userId), CancellationToken.None);
            if (response.Value == null)
            {
                return NotFound();
            }
            return Ok(response.Value);
        }

        /// <summary>
        /// Updates an existing media item.
        /// </summary>
        /// <param name="id">The ID of the media item to update.</param>
        /// <param name="command">The command containing the updated media item details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMediaItemCommand command)
        {
            if (id != command.MediaItemId)
            {
                return BadRequest();
            }
            var userId = GetUserId();
            var updateCommand = command with { UserId = userId };
            await _mediaItemService.UpdateMediaItemAsync(updateCommand, CancellationToken.None);
            return NoContent();
        }

        /// <summary>
        /// Deletes a media item.
        /// </summary>
        /// <param name="id">The ID of the media item to delete.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            await _mediaItemService.DeleteMediaItemAsync(new DeleteMediaItemCommand(id, userId), CancellationToken.None);
            return NoContent();
        }

        /// <summary>
        /// Retrieves a list of media items.
        /// </summary>
        /// <param name="query">The query containing filtering, searching, and pagination parameters.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ListMediaItemsQuery query)
        {
            var userId = GetUserId();
            var listQuery = query with { UserId = userId };
            OperationResult<ListMediaItemsResponse> response = await _mediaItemService.ListMediaItemsAsync(listQuery, CancellationToken.None);
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