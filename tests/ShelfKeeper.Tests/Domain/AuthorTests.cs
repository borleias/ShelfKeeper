// <copyright file="AuthorTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;

using ShelfKeeper.Shared.Common;


namespace ShelfKeeper.Tests.Domain
{
    public class AuthorTests
    {
        [Fact]
        public void Author_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid authorId = Guid.NewGuid();
            string name = "J.R.R. Tolkien";

            // Act
            Author author = new Author
            {
                Id = authorId,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(authorId, author.Id);
            Assert.Equal(name, author.Name);
            Assert.NotEqual(default(DateTime), author.CreatedAt);
            Assert.NotEqual(default(DateTime), author.LastUpdatedAt);

            OperationResult validationOperationResult = author.Validate();
            Assert.True(validationOperationResult.IsSuccess);
        }

        [Fact]
        public void Author_ValidationFails_WhenNameIsEmpty()
        {
            // Arrange
            Author author = new Author
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = author.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "Author name cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}