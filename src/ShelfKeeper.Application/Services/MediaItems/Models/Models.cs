// <copyright file="Models.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Application.Services.MediaItems.Models
{
    /// <summary>
    /// Represents a command to create a new media item.
    /// </summary>
    /// <param name="UserId">The ID of the user who owns the media item.</param>
    /// <param name="Title">The title of the media item.</param>
    /// <param name="Type">The type of the media item (e.g., Book, CD, DVD).</param>
    /// <param name="Year">The publication or release year of the media item.</param>
    /// <param name="IsbnUpc">The ISBN or UPC of the media item.</param>
    /// <param name="Notes">Personal notes about the media item.</param>
    /// <param name="Progress">The current progress (e.g., page number for a book).</param>
    /// <param name="LocationId">The optional ID of the physical location of the media item.</param>
    /// <param name="AuthorId">The optional ID of the author or artist of the media item.</param>
    public record CreateMediaItemCommand(
        Guid UserId,
        string Title,
        string Type,
        int? Year,
        string? IsbnUpc,
        string Notes,
        string? Progress,
        Guid? LocationId,
        Guid? AuthorId
    );

    /// <summary>
    /// Represents the response after creating a media item.
    /// </summary>
    /// <param name="MediaItemId">The ID of the newly created media item.</param>
    /// <param name="Title">The title of the newly created media item.</param>
    /// <param name="Type">The type of the newly created media item.</param>
    /// <param name="IsbnUpc">The ISBN or UPC of the newly created media item.</param>
    public record CreateMediaItemResponse(Guid MediaItemId, string Title, string Type, string? IsbnUpc);

    /// <summary>
    /// Represents a query to retrieve a media item by its ID.
    /// </summary>
    /// <param name="MediaItemId">The ID of the media item to retrieve.</param>
    /// <param name="UserId">The ID of the user who owns the media item.</param>
    public record GetMediaItemByIdQuery(Guid MediaItemId, Guid UserId);

    /// <summary>
    /// Represents the response containing details of a media item.
    /// </summary>
    /// <param name="MediaItemId">The ID of the media item.</param>
    /// <param name="Title">The title of the media item.</param>
    /// <param name="Type">The type of the media item.</param>
    /// <param name="Year">The publication or release year.</param>
    /// <param name="IsbnUpc">The ISBN or UPC.</param>
    /// <param name="Notes">Personal notes.</param>
    /// <param name="Progress">Current progress.</param>
    /// <param name="AddedAt">Date and time when the media item was added.</param>
    /// <param name="LocationId">The optional ID of the physical location.</param>
    /// <param name="LocationTitle">The title of the physical location.</param>
    /// <param name="AuthorId">The optional ID of the author or artist.</param>
    /// <param name="AuthorName">The name of the author or artist.</param>
    public record GetMediaItemByIdResponse(
        Guid MediaItemId,
        string Title,
        string Type,
        int? Year,
        string? IsbnUpc,
        string Notes,
        string? Progress,
        DateTime AddedAt,
        Guid? LocationId,
        string LocationTitle,
        Guid? AuthorId,
        string AuthorName
    );

    /// <summary>
    /// Represents a command to update an existing media item.
    /// </summary>
    /// <param name="MediaItemId">The ID of the media item to update.</param>
    /// <param name="UserId">The ID of the user who owns the media item.</param>
    /// <param name="Title">The updated title of the media item.</param>
    /// <param name="Type">The updated type of the media item.</param>
    /// <param name="Year">The updated publication or release year.</param>
    /// <param name="IsbnUpc">The updated ISBN or UPC.</param>
    /// <param name="Notes">Updated personal notes.</param>
    /// <param name="Progress">Updated current progress.</param>
    /// <param name="LocationId">The updated optional ID of the physical location.</param>
    /// <param name="AuthorId">The updated optional ID of the author or artist.</param>
    public record UpdateMediaItemCommand(
        Guid MediaItemId,
        Guid UserId,
        string Title,
        string Type,
        int? Year,
        string? IsbnUpc,
        string Notes,
        string? Progress,
        Guid? LocationId,
        Guid? AuthorId
    );

    /// <summary>
    /// Represents a command to delete a media item.
    /// </summary>
    /// <param name="MediaItemId">The ID of the media item to delete.</param>
    /// <param name="UserId">The ID of the user who owns the media item.</param>
    public record DeleteMediaItemCommand(Guid MediaItemId, Guid UserId);

    /// <summary>
    /// Represents a query to list media items with optional filtering and pagination.
    /// </summary>
    /// <param name="UserId">The ID of the user whose media items to list.</param>
    /// <param name="SearchTerm">Optional search term to filter media items by title, author, notes, or ISBN/UPC.</param>
    /// <param name="TypeFilter">Optional type filter to narrow down media items by type (e.g., Book, CD).</param>
    /// <param name="PageNumber">The page number for pagination (defaults to 1).</param>
    /// <param name="PageSize">The number of items per page for pagination (defaults to 10).</param>
    /// <param name="SortBy">Optional field to sort by (e.g., "Title", "AddedAt").</param>
    /// <param name="SortOrder">Optional sort order (Ascending or Descending). Defaults to Ascending.</param>
    public record ListMediaItemsQuery(
        Guid UserId,
        string SearchTerm = null,
        string TypeFilter = null,
        int PageNumber = 1,
        int PageSize = 10,
        string SortBy = null,
        SortOrder SortOrder = SortOrder.Ascending
    );

    /// <summary>
    /// Represents the sort order for queries.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Ascending sort order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Descending sort order.
        /// </summary>
        Descending
    }

    /// <summary>
    /// Represents the response containing a paginated list of media items.
    /// </summary>
    /// <param name="MediaItems">A collection of <see cref="MediaItemDto"/> representing the media items on the current page.</param>
    /// <param name="TotalCount">The total number of media items matching the query.</param>
    /// <param name="PageNumber">The current page number.</param>
    /// <param name="PageSize">The number of items per page.</param>
    public record ListMediaItemsResponse(
        IEnumerable<MediaItemDto> MediaItems,
        int TotalCount,
        int PageNumber,
        int PageSize
    );

    /// <summary>
    /// Represents a data transfer object for a media item.
    /// </summary>
    /// <param name="MediaItemId">The ID of the media item.</param>
    /// <param name="Title">The title of the media item.</param>
    /// <param name="Type">The type of the media item.</param>
    /// <param name="Year">The publication or release year.</param>
    /// <param name="IsbnUpc">The ISBN or UPC.</param>
    /// <param name="Notes">Personal notes.</param>
    /// <param name="Progress">Current progress.</param>
    /// <param name="AddedAt">Date and time when the media item was added.</param>
    /// <param name="LocationId">The optional ID of the physical location.</param>
    /// <param name="LocationTitle">The title of the physical location.</param>
    /// <param name="AuthorId">The optional ID of the author or artist.</param>
    /// <param name="AuthorName">The name of the author or artist.</param>
    public record MediaItemDto(
        Guid MediaItemId,
        string Title,
        string Type,
        int? Year,
        string? IsbnUpc,
        string Notes,
        string? Progress,
        DateTime AddedAt,
        Guid? LocationId,
        string LocationTitle,
        Guid? AuthorId,
        string AuthorName
    );
}