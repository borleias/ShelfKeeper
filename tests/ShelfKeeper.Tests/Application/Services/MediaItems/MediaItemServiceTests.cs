// <copyright file="MediaItemServiceTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Application.Services.MediaItems;
using ShelfKeeper.Application.Services.MediaItems.Models;
using ShelfKeeper.Domain.Entities;
using Moq.EntityFrameworkCore;
using System.Linq;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.Application.Services.MediaItems
{
    public class MediaItemServiceTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly MediaItemService _mediaItemService;

        public MediaItemServiceTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mediaItemService = new MediaItemService(_mockContext.Object);
        }

        [Fact]
        public async Task CreateMediaItemAsync_ShouldCreateMediaItemAndReturnResponse()
        {
            // Arrange
            CreateMediaItemCommand command = new CreateMediaItemCommand(
                UserId: Guid.NewGuid(),
                Title: "Test Book",
                Type: "Book",
                Year: 2023,
                IsbnUpc: "1234567890",
                Notes: "Some notes",
                Progress: "Page 10",
                LocationId: null,
                AuthorId: null
            );
            List<MediaItem> mediaItems = new List<MediaItem>();

            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(mediaItems);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            Shared.Common.OperationResult<CreateMediaItemResponse> result = await _mediaItemService.CreateMediaItemAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockContext.Verify(c => c.MediaItems.Add(It.IsAny<MediaItem>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(command.Title, result.Value.Title);
            Assert.Equal(command.Type, result.Value.Type);
            Assert.NotEqual(Guid.Empty, result.Value.MediaItemId);
            Assert.Single(mediaItems);
            Assert.Equal(result.Value.MediaItemId, mediaItems[0].Id);
        }

        [Fact]
        public async Task GetMediaItemByIdAsync_ShouldReturnMediaItem()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid mediaItemId = Guid.NewGuid();
            MediaItem mediaItem = new MediaItem { Id = mediaItemId, UserId = userId, Title = "Test Book", Type = "Book", AddedAt = DateTime.UtcNow, Location = new Location(), Author = new Author() };

            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(new List<MediaItem> { mediaItem });

            // Act
            Shared.Common.OperationResult<GetMediaItemByIdResponse> result = await _mediaItemService.GetMediaItemByIdAsync(new GetMediaItemByIdQuery(mediaItemId, userId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(mediaItemId, result.Value.MediaItemId);
            Assert.Equal("Test Book", result.Value.Title);
        }

        [Fact]
        public async Task GetMediaItemByIdAsync_ShouldReturnNullIfNotFound()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid mediaItemId = Guid.NewGuid();

            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(new List<MediaItem>());

            // Act
            Shared.Common.OperationResult<GetMediaItemByIdResponse> result = await _mediaItemService.GetMediaItemByIdAsync(new GetMediaItemByIdQuery(mediaItemId, userId), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Type == OperationErrorType.NotFoundError);
        }

        [Fact]
        public async Task UpdateMediaItemAsync_ShouldUpdateMediaItemAndSave()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid mediaItemId = Guid.NewGuid();
            MediaItem mediaItem = new MediaItem { Id = mediaItemId, UserId = userId, Title = "Old Title", Type = "Book", AddedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(new List<MediaItem> { mediaItem });
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            UpdateMediaItemCommand command = new UpdateMediaItemCommand(
                MediaItemId: mediaItemId,
                UserId: userId,
                Title: "New Title",
                Type: "Book",
                Year: 2024,
                IsbnUpc: "0987654321",
                Notes: "Updated notes",
                Progress: "Finished",
                LocationId: null,
                AuthorId: null
            );

            // Act
            Shared.Common.OperationResult result = await _mediaItemService.UpdateMediaItemAsync(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal("New Title", mediaItem.Title);
            Assert.Equal(2024, mediaItem.Year);
        }

        [Fact]
        public async Task DeleteMediaItemAsync_ShouldDeleteMediaItemAndSave()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid mediaItemId = Guid.NewGuid();
            MediaItem mediaItem = new MediaItem { Id = mediaItemId, UserId = userId, Title = "Test Book", Type = "Book", AddedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(new List<MediaItem> { mediaItem });
            _mockContext.Setup(c => c.MediaItems.Remove(It.IsAny<MediaItem>()));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            OperationResult operationResult = await _mediaItemService.DeleteMediaItemAsync(new DeleteMediaItemCommand(mediaItemId, userId), CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            _mockContext.Verify(c => c.MediaItems.Remove(It.IsAny<MediaItem>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ListMediaItemsAsync_ShouldReturnFilteredAndSortedMediaItems()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            List<MediaItem> mediaItems = new List<MediaItem>
            {
                new MediaItem { Id = Guid.NewGuid(), UserId = userId, Title = "Book A", Type = "Book", AddedAt = DateTime.UtcNow.AddDays(-2), Author = new Author { Name = "Author X" }, Location = new Location(), CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow },
                new MediaItem { Id = Guid.NewGuid(), UserId = userId, Title = "Book B", Type = "Book", AddedAt = DateTime.UtcNow.AddDays(-1), Author = new Author { Name = "Author Y" }, Location = new Location(), CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow },
                new MediaItem { Id = Guid.NewGuid(), UserId = userId, Title = "Movie C", Type = "Movie", AddedAt = DateTime.UtcNow.AddDays(-3), Author = new Author { Name = "Author Z" }, Location = new Location(), CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow }
            };

            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(mediaItems);

            ListMediaItemsQuery query = new ListMediaItemsQuery(UserId: userId, TypeFilter: "Book", SortBy: "AddedAt", SortOrder: SortOrder.Descending);

            // Act
            OperationResult<ListMediaItemsResponse> operationResult = await _mediaItemService.ListMediaItemsAsync(query, CancellationToken.None);

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.NotNull(operationResult.Value);
            Assert.Equal(2, operationResult.Value.TotalCount);
            Assert.Equal("Book B", operationResult.Value.MediaItems.First().Title);
            Assert.Equal("Book A", operationResult.Value.MediaItems.Last().Title);
        }
    }
}
