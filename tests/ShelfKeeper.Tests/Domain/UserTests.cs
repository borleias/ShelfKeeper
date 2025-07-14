// <copyright file="UserTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using ShelfKeeper.Domain.Entities;

using ShelfKeeper.Shared.Common;


namespace ShelfKeeper.Tests.Domain
{
    public class UserTests
    {
        [Fact]
        public void User_CanBeCreatedWithValidData()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string email = "test@example.com";
            string passwordHash = "hashedpassword";
            string name = "Test User";

            // Act
            User user = new User
            {
                Id = userId,
                Email = email,
                PasswordHash = passwordHash,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(userId, user.Id);
            Assert.Equal(email, user.Email);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.Equal(name, user.Name);
            Assert.NotEqual(default(DateTime), user.CreatedAt);
            Assert.NotEqual(default(DateTime), user.LastUpdatedAt);

            OperationResult validationOperationResult = user.Validate();
            Assert.True(validationOperationResult.IsSuccess);
        }

        [Fact]
        public void User_ValidationFails_WhenEmailIsEmpty()
        {
            // Arrange
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = string.Empty,
                PasswordHash = "hashedpassword",
                Name = "Test User",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = user.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "User email cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void User_ValidationFails_WhenPasswordHashIsEmpty()
        {
            // Arrange
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = string.Empty,
                Name = "Test User",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = user.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "User password hash cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }

        [Fact]
        public void User_ValidationFails_WhenNameIsEmpty()
        {
            // Arrange
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Name = string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Act
            OperationResult validationOperationResult = user.Validate();

            // Assert
            Assert.True(validationOperationResult.IsFailure);
            Assert.Contains(validationOperationResult.Errors, e => e.Message == "User name cannot be empty." && e.Type == OperationErrorType.ValidationError);
        }
    }
}