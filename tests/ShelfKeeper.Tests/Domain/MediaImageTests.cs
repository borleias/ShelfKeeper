// <copyright file="MediaImageTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Shared.Common;
using System.Linq;

namespace ShelfKeeper.Tests.Domain
{
    public class MediaImageTests
    {
        [Fact]
        public void MediaImage_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid imageId = Guid.NewGuid();
            Guid mediaItemId = Guid.NewGuid();
            string imageUrl = "http://example.com/image.jpg";

            // Act
            MediaImage mediaImage = new MediaImage
            {
                Id = imageId,
                MediaItemId = mediaItemId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(imageId, mediaImage.Id);
            Assert.Equal(mediaItemId, mediaImage.MediaItemId);
            Assert.Equal(imageUrl, mediaImage.ImageUrl);
            Assert.NotEqual(default(DateTime), mediaImage.CreatedAt);
            Assert.NotEqual(default(DateTime), mediaImage.LastUpdatedAt);

            OperationResult validationResult = mediaImage.Validate();
            Assert.True(validationResult.IsSuccess);
        }

        [Fact]
        public void MediaImage_ValidationFails_WhenMediaItemIdIsEmpty()
        {
            // Arrange
            MediaImage mediaImage = new MediaImage
            {
                Id = Guid.NewGuid(),
                MediaItemId = Guid.Empty,
                ImageUrl = "http://example.com/image.jpg",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationResult = mediaImage.Validate();

            // Assert
            Assert.True(validationResult.IsFailure);
            Assert.Contains(validationResult.Errors, e => e.Message == "Media item ID cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void MediaImage_ValidationFails_WhenImageUrlIsEmpty()
        {
            // Arrange
            MediaImage mediaImage = new MediaImage
            {
                Id = Guid.NewGuid(),
                MediaItemId = Guid.NewGuid(),
                ImageUrl = string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationResult = mediaImage.Validate();

            // Assert
            Assert.True(validationResult.IsFailure);
            Assert.Contains(validationResult.Errors, e => e.Message == "Image URL cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}