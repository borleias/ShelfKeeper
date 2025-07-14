// <copyright file="AuditableEntityTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Common;

using System.Linq;
using ShelfKeeper.Shared.Common;


namespace ShelfKeeper.Tests.Domain
{
    public class AuditableEntityTests
    {
        private class TestAuditableEntity : AuditableEntity { }

        [Fact]
        public void AuditableEntity_SetsCreationAndLastUpdateDates()
        {
            // Arrange & Act
            TestAuditableEntity entity = new TestAuditableEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.NotEqual(default(Guid), entity.Id);
            Assert.NotEqual(default(DateTime), entity.CreatedAt);
            Assert.NotEqual(default(DateTime), entity.LastUpdatedAt);
        }

        [Fact]
        public void Validate_ReturnsFailure_WhenIdIsEmpty()
        {
            // Arrange
            TestAuditableEntity entity = new TestAuditableEntity
            {
                Id = Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult operationResult = entity.Validate();

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "Entity Id cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void Validate_ReturnsFailure_WhenCreatedAtIsDefault()
        {
            // Arrange
            TestAuditableEntity entity = new TestAuditableEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = default(DateTime),
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult operationResult = entity.Validate();

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "CreatedAt date cannot be default." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void Validate_ReturnsFailure_WhenLastUpdatedAtIsDefault()
        {
            // Arrange
            TestAuditableEntity entity = new TestAuditableEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = default(DateTime)
            };

            // Act
            OperationResult operationResult = entity.Validate();

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "LastUpdatedAt date cannot be default." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void Validate_ReturnsFailure_WhenLastUpdatedAtIsBeforeCreatedAt()
        {
            // Arrange
            TestAuditableEntity entity = new TestAuditableEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            OperationResult operationResult = entity.Validate();

            // Assert
            Assert.True(operationResult.IsFailure);
            Assert.Contains(operationResult.Errors, e => e.Message == "LastUpdatedAt cannot be before CreatedAt." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void Validate_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            TestAuditableEntity entity = new TestAuditableEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult operationResult = entity.Validate();

            // Assert
            Assert.True(operationResult.IsSuccess);
            Assert.Empty(operationResult.Errors);
        }
    }
}
