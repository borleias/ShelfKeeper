// <copyright file="IMediaItemService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Services.MediaItems.Models;

namespace ShelfKeeper.Application.Services.MediaItems
{
    /// <summary>
    /// Defines the interface for a media item management service.
    /// </summary>
    public interface IMediaItemService
    {
        /// <summary>
        /// Creates a new media item asynchronously.
        /// </summary>
        /// <param name="command">The command containing the media item details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the response with the new media item's details.</returns>
        Task<CreateMediaItemResponse> CreateMediaItemAsync(CreateMediaItemCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a media item by its ID asynchronously.
        /// </summary>
        /// <param name="query">The query containing the media item ID and user ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the media item details, or null if not found.</returns>
        Task<GetMediaItemByIdResponse> GetMediaItemByIdAsync(GetMediaItemByIdQuery query, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing media item asynchronously.
        /// </summary>
        /// <param name="command">The command containing the updated media item details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateMediaItemAsync(UpdateMediaItemCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a media item asynchronously.
        /// </summary>
        /// <param name="command">The command containing the ID of the media item to delete and the user ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteMediaItemAsync(DeleteMediaItemCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a list of media items based on the provided query parameters asynchronously.
        /// </summary>
        /// <param name="query">The query containing filtering, searching, and pagination parameters.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a paginated list of media items.</returns>
        Task<ListMediaItemsResponse> ListMediaItemsAsync(ListMediaItemsQuery query, CancellationToken cancellationToken);
    }
}