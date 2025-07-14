// <copyright file="LocationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;

using ShelfKeeper.Shared.Common;


namespace ShelfKeeper.Tests.Domain
{
    public class LocationTests
    {
        [Fact]
        public void Location_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid locationId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            string title = "Living Room Shelf";
            string description = "The big one next to the TV.";

            // Act
            Location location = new Location
            {
                Id = locationId,
                UserId = userId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(locationId, location.Id);
            Assert.Equal(userId, location.UserId);
            Assert.Equal(title, location.Title);
            Assert.Equal(description, location.Description);
            Assert.NotEqual(default(DateTime), location.CreatedAt);
            Assert.NotEqual(default(DateTime), location.LastUpdatedAt);

            OperationResult validationOperationResult = location.Validate();
            Assert.True(validationOperationResult.IsSuccess);
        }

        [Fact]
        public void Location_ValidationFails_WhenTitleIsEmpty()
        {
            // Arrange
            Location location = new Location
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = string.Empty,
                Description = "Some description",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = location.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "Location title cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}