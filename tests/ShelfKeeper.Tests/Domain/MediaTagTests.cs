// <copyright file="MediaTagTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Shared.Common;
using System.Linq;

namespace ShelfKeeper.Tests.Domain
{
    public class MediaTagTests
    {
        [Fact]
        public void MediaTag_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid tagId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string name = "Fantasy";

            // Act
            MediaTag mediaTag = new MediaTag
            {
                Id = tagId,
                UserId = userId,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(tagId, mediaTag.Id);
            Assert.Equal(userId, mediaTag.UserId);
            Assert.Equal(name, mediaTag.Name);
            Assert.NotEqual(default(DateTime), mediaTag.CreatedAt);
            Assert.NotEqual(default(DateTime), mediaTag.LastUpdatedAt);

            OperationResult validationResult = mediaTag.Validate();
            Assert.True(validationResult.IsSuccess);
        }

        [Fact]
        public void MediaTag_ValidationFails_WhenUserIdIsEmpty()
        {
            // Arrange
            MediaTag mediaTag = new MediaTag
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty,
                Name = "Fantasy",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationResult = mediaTag.Validate();

            // Assert
            Assert.True(validationResult.IsFailure);
            Assert.Contains(validationResult.Errors, e => e.Message == "User ID cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void MediaTag_ValidationFails_WhenNameIsEmpty()
        {
            // Arrange
            MediaTag mediaTag = new MediaTag
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationResult = mediaTag.Validate();

            // Assert
            Assert.True(validationResult.IsFailure);
            Assert.Contains(validationResult.Errors, e => e.Message == "Media tag name cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}