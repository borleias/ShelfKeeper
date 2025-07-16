// <copyright file="FeatureGateServiceTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Application.Services.FeatureGates;
using ShelfKeeper.Application.Services.Subscriptions;
using ShelfKeeper.Application.Services.Subscriptions.Models;
using ShelfKeeper.Domain.Entities;
using Moq.EntityFrameworkCore;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Tests.Application.Services.FeatureGates
{
    public class FeatureGateServiceTests
    {
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly FeatureGateService _featureGateService;

        public FeatureGateServiceTests()
        {
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _mockContext = new Mock<IApplicationDbContext>();
            _featureGateService = new FeatureGateService(_mockSubscriptionService.Object, _mockContext.Object);
        }

        [Fact]
        public async Task HasAccessAsync_MediaItemLimit_FreePlanExceeded_ShouldReturnFailure()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Free, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            var mediaItems = new List<MediaItem>();
            for (int i = 0; i < 10; i++) // Free plan limit is 10
            {
                mediaItems.Add(new MediaItem { Id = Guid.NewGuid(), UserId = userId });
            }
            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(mediaItems);

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.MediaItemLimit, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Message.Contains("Media item limit (10) exceeded for Free plan."));
        }

        [Fact]
        public async Task HasAccessAsync_MediaItemLimit_FreePlanNotExceeded_ShouldReturnSuccess()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Free, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            var mediaItems = new List<MediaItem>();
            for (int i = 0; i < 5; i++) // Below Free plan limit
            {
                mediaItems.Add(new MediaItem { Id = Guid.NewGuid(), UserId = userId });
            }
            _mockContext.Setup(c => c.MediaItems).ReturnsDbSet(mediaItems);

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.MediaItemLimit, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HasAccessAsync_SharedLists_PremiumPlan_ShouldReturnSuccess()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.SharedLists, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HasAccessAsync_SharedLists_FreePlan_ShouldReturnFailure()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Free, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.SharedLists, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Message.Contains("Shared lists require Premium plan."));
        }

        [Fact]
        public async Task HasAccessAsync_AdvancedSearch_BasicPlan_ShouldReturnSuccess()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Basic, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.AdvancedSearch, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HasAccessAsync_AdvancedSearch_FreePlan_ShouldReturnFailure()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Free, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.AdvancedSearch, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Message.Contains("Advanced search requires Basic or Premium plan."));
        }

        [Fact]
        public async Task HasAccessAsync_BatchOperations_PremiumPlan_ShouldReturnSuccess()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.BatchOperations, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HasAccessAsync_BatchOperations_FreePlan_ShouldReturnFailure()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Free, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.BatchOperations, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Message.Contains("Batch operations require Premium plan."));
        }

        [Fact]
        public async Task HasAccessAsync_CsvImportExport_PremiumPlan_ShouldReturnSuccess()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Premium, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.CsvImportExport, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HasAccessAsync_CsvImportExport_FreePlan_ShouldReturnFailure()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var subscriptionDto = new SubscriptionDto(Guid.NewGuid(), userId, SubscriptionPlan.Free, SubscriptionStatus.Active, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), false);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<SubscriptionDto>.Success(subscriptionDto));

            // Act
            OperationResult result = await _featureGateService.HasAccessAsync(userId, FeatureType.CsvImportExport, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Message.Contains("CSV import/export requires Premium plan."));
        }
    }
}
