// <copyright file="MediaItemService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Application.Services.MediaItems.Models;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Application.Services.MediaItems
{
    /// <summary>
    /// Provides services for managing media items.
    /// </summary>
    public class MediaItemService : IMediaItemService
    {
        private readonly IApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaItemService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public MediaItemService(IApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new media item.
        /// </summary>
        /// <param name="command">The command containing the media item details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains the response with the new media item's details.</returns>
        public async Task<OperationResult<CreateMediaItemResponse>> CreateMediaItemAsync(CreateMediaItemCommand command, CancellationToken cancellationToken)
        {
            MediaItem mediaItem = new MediaItem
            {
                UserId = command.UserId,
                Title = command.Title,
                Type = command.Type,
                Year = command.Year,
                IsbnUpc = command.IsbnUpc,
                Notes = command.Notes,
                Progress = command.Progress,
                AddedAt = DateTime.UtcNow,
                LocationId = command.LocationId,
                AuthorId = command.AuthorId
            };

            _context.MediaItems.Add(mediaItem);
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult<CreateMediaItemResponse>.Success(new CreateMediaItemResponse(mediaItem.Id, mediaItem.Title, mediaItem.Type, mediaItem.IsbnUpc));
        }

        /// <summary>
        /// Retrieves a media item by its ID and user ID.
        /// </summary>
        /// <param name="query">The query containing the media item ID and user ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains the media item details, or null if not found.</returns>
        public async Task<OperationResult<GetMediaItemByIdResponse>> GetMediaItemByIdAsync(GetMediaItemByIdQuery query, CancellationToken cancellationToken)
        {
            MediaItem mediaItem = await _context.MediaItems
                .Include(mi => mi.Location)
                .Include(mi => mi.Author)
                .FirstOrDefaultAsync(mi => mi.Id == query.MediaItemId && mi.UserId == query.UserId, cancellationToken);

            if (mediaItem == null)
            {
                return null; // Or throw NotFoundException
            }

            return OperationResult<GetMediaItemByIdResponse>.Success(new GetMediaItemByIdResponse(
                mediaItem.Id,
                mediaItem.Title,
                mediaItem.Type,
                mediaItem.Year,
                mediaItem.IsbnUpc,
                mediaItem.Notes,
                mediaItem.Progress,
                mediaItem.AddedAt,
                mediaItem.LocationId,
                mediaItem.Location?.Title,
                mediaItem.AuthorId,
                mediaItem.Author?.Name)
            );
        }

        /// <summary>
        /// Updates an existing media item.
        /// </summary>
        /// <param name="command">The command containing the updated media item details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<OperationResult> UpdateMediaItemAsync(UpdateMediaItemCommand command, CancellationToken cancellationToken)
        {
            MediaItem mediaItem = await _context.MediaItems
                .FirstOrDefaultAsync(mi => mi.Id == command.MediaItemId && mi.UserId == command.UserId, cancellationToken);

            if (mediaItem == null)
            {
                return OperationResult.Failure("Not found", OperationErrorType.NotFoundError); // Or throw NotFoundException
            }

            mediaItem.Title = command.Title;
            mediaItem.Type = command.Type;
            mediaItem.Year = command.Year;
            mediaItem.IsbnUpc = command.IsbnUpc;
            mediaItem.Notes = command.Notes;
            mediaItem.Progress = command.Progress;
            mediaItem.LocationId = command.LocationId;
            mediaItem.AuthorId = command.AuthorId;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Deletes a media item.
        /// </summary>
        /// <param name="command">The command containing the ID of the media item to delete and the user ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<OperationResult> DeleteMediaItemAsync(DeleteMediaItemCommand command, CancellationToken cancellationToken)
        {
            MediaItem mediaItem = await _context.MediaItems
                .FirstOrDefaultAsync(mi => mi.Id == command.MediaItemId && mi.UserId == command.UserId, cancellationToken);

            if (mediaItem == null)
            {
                return OperationResult.Failure("Not found", OperationErrorType.NotFoundError);
            }

            _context.MediaItems.Remove(mediaItem);
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Retrieves a list of media items based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query containing filtering, searching, and pagination parameters.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a paginated list of media items.</returns>
        public async Task<OperationResult<ListMediaItemsResponse>> ListMediaItemsAsync(ListMediaItemsQuery query, CancellationToken cancellationToken)
        {
            IQueryable<MediaItem> mediaItemsQuery = _context.MediaItems
                .Include(mi => mi.Location)
                .Include(mi => mi.Author)
                .Where(mi => mi.UserId == query.UserId);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
                {
                    case "title":
                        mediaItemsQuery = query.SortOrder == SortOrder.Ascending ? mediaItemsQuery.OrderBy(mi => mi.Title) : mediaItemsQuery.OrderByDescending(mi => mi.Title);
                        break;
                    case "addedat":
                        mediaItemsQuery = query.SortOrder == SortOrder.Ascending ? mediaItemsQuery.OrderBy(mi => mi.AddedAt) : mediaItemsQuery.OrderByDescending(mi => mi.AddedAt);
                        break;
                    case "year":
                        mediaItemsQuery = query.SortOrder == SortOrder.Ascending ? mediaItemsQuery.OrderBy(mi => mi.Year) : mediaItemsQuery.OrderByDescending(mi => mi.Year);
                        break;
                    case "author":
                        mediaItemsQuery = query.SortOrder == SortOrder.Ascending ? mediaItemsQuery.OrderBy(mi => mi.Author.Name) : mediaItemsQuery.OrderByDescending(mi => mi.Author.Name);
                        break;
                    default:
                        mediaItemsQuery = mediaItemsQuery.OrderByDescending(mi => mi.AddedAt); // Default sort
                        break;
                }
            }
            else
            {
                mediaItemsQuery = mediaItemsQuery.OrderByDescending(mi => mi.AddedAt); // Default sort if no SortBy is provided
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                mediaItemsQuery = mediaItemsQuery.Where(mi =>
                    mi.Title.Contains(query.SearchTerm) ||
                    mi.Author.Name.Contains(query.SearchTerm) ||
                    mi.Notes.Contains(query.SearchTerm) ||
                    mi.IsbnUpc.Contains(query.SearchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(query.TypeFilter))
            {
                mediaItemsQuery = mediaItemsQuery.Where(mi => mi.Type == query.TypeFilter);
            }

            int totalCount = await mediaItemsQuery.CountAsync(cancellationToken);

            List<MediaItemDto> mediaItems = await mediaItemsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(mi => new MediaItemDto(
                    mi.Id,
                    mi.Title,
                    mi.Type,
                    mi.Year,
                    mi.IsbnUpc,
                    mi.Notes,
                    mi.Progress,
                    mi.AddedAt,
                    mi.LocationId,
                    mi.Location.Title,
                    mi.AuthorId,
                    mi.Author.Name
                ))
                .ToListAsync(cancellationToken);

            return OperationResult<ListMediaItemsResponse>.Success(new ListMediaItemsResponse(mediaItems, totalCount, query.PageNumber, query.PageSize));
        }
    }
}