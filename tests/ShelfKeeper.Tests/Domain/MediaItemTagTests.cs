// <copyright file="MediaItemTagTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Shared.Common;
using System.Linq;

namespace ShelfKeeper.Tests.Domain
{
    public class MediaItemTagTests
    {
        [Fact]
        public void MediaItemTag_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid mediaItemId = Guid.NewGuid();
            Guid mediaTagId = Guid.NewGuid();

            // Act
            MediaItemTag mediaItemTag = new MediaItemTag
            {
                MediaItemId = mediaItemId,
                MediaTagId = mediaTagId
            };

            // Assert
            Assert.Equal(mediaItemId, mediaItemTag.MediaItemId);
            Assert.Equal(mediaTagId, mediaItemTag.MediaTagId);

            OperationResult validationResult = mediaItemTag.Validate();
            Assert.True(validationResult.IsSuccess);
        }

        [Fact]
        public void MediaItemTag_ValidationFails_WhenMediaItemIdIsEmpty()
        {
            // Arrange
            MediaItemTag mediaItemTag = new MediaItemTag
            {
                MediaItemId = Guid.Empty,
                MediaTagId = Guid.NewGuid()
            };

            // Act
            OperationResult validationResult = mediaItemTag.Validate();

            // Assert
            Assert.True(validationResult.IsFailure);
            Assert.Contains(validationResult.Errors, e => e.Message == "Media item ID cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void MediaItemTag_ValidationFails_WhenMediaTagIdIsEmpty()
        {
            // Arrange
            MediaItemTag mediaItemTag = new MediaItemTag
            {
                MediaItemId = Guid.NewGuid(),
                MediaTagId = Guid.Empty
            };

            // Act
            OperationResult validationResult = mediaItemTag.Validate();

            // Assert
            Assert.True(validationResult.IsFailure);
            Assert.Contains(validationResult.Errors, e => e.Message == "Media tag ID cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}