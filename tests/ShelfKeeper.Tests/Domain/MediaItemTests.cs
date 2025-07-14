// <copyright file="MediaItemTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;

using ShelfKeeper.Shared.Common;


namespace ShelfKeeper.Tests.Domain
{
    public class MediaItemTests
    {
        [Fact]
        public void MediaItem_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid mediaItemId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string title = "The Hobbit";
            string type = "Book";
            int year = 1937;
            string isbnUpc = "978-0345339683";
            string notes = "First edition copy.";
            string progress = "Read";
            DateTime addedAt = DateTime.UtcNow;
            Guid locationId = Guid.NewGuid();
            Guid authorId = Guid.NewGuid();

            // Act
            MediaItem mediaItem = new MediaItem
            {
                Id = mediaItemId,
                UserId = userId,
                Title = title,
                Type = type,
                Year = year,
                IsbnUpc = isbnUpc,
                Notes = notes,
                Progress = progress,
                AddedAt = addedAt,
                LocationId = locationId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(mediaItemId, mediaItem.Id);
            Assert.Equal(userId, mediaItem.UserId);
            Assert.Equal(title, mediaItem.Title);
            Assert.Equal(type, mediaItem.Type);
            Assert.Equal(year, mediaItem.Year);
            Assert.Equal(isbnUpc, mediaItem.IsbnUpc);
            Assert.Equal(notes, mediaItem.Notes);
            Assert.Equal(progress, mediaItem.Progress);
            Assert.Equal(addedAt, mediaItem.AddedAt);
            Assert.Equal(locationId, mediaItem.LocationId);
            Assert.Equal(authorId, mediaItem.AuthorId);
            Assert.NotEqual(default(DateTime), mediaItem.CreatedAt);
            Assert.NotEqual(default(DateTime), mediaItem.LastUpdatedAt);

            OperationResult validationOperationResult = mediaItem.Validate();
            Assert.True(validationOperationResult.IsSuccess);
        }

        [Fact]
        public void MediaItem_ValidationFails_WhenTitleIsEmpty()
        {
            // Arrange
            MediaItem mediaItem = new MediaItem
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = string.Empty,
                Type = "Book",
                AddedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = mediaItem.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "Media item title cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void MediaItem_ValidationFails_WhenTypeIsEmpty()
        {
            // Arrange
            MediaItem mediaItem = new MediaItem
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Test Title",
                Type = string.Empty,
                AddedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = mediaItem.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "Media item type cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}
