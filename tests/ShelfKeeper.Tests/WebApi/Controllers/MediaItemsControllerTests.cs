// <copyright file="MediaItemsControllerTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Moq;
using Xunit;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShelfKeeper.Application.Services.MediaItems;
using ShelfKeeper.Application.Services.MediaItems.Models;
using ShelfKeeper.Domain.Common;
using ShelfKeeper.WebApi.Controllers;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Tests.WebApi.Controllers
{
    public class MediaItemsControllerTests
    {
        private readonly Mock<IMediaItemService> _mockMediaItemService;
        private readonly MediaItemsController _controller;
        private readonly Guid _userId;

        public MediaItemsControllerTests()
        {
            _mockMediaItemService = new Mock<IMediaItemService>();
            _controller = new MediaItemsController(_mockMediaItemService.Object);
            _userId = Guid.NewGuid();
            
            // Set up default user identity
            SetupUserIdentity(_controller, _userId);
        }

        [Fact]
        public async Task Create_WithValidCommand_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var command = new CreateMediaItemCommand(
                Guid.Empty, 
                "Test Book", 
                "Book", 
                2023, 
                "9781234567890", 
                "Description", 
                null, 
                null, 
                null);
            var expectedCommand = command with { UserId = _userId };
            var response = new CreateMediaItemResponse(Guid.NewGuid(), "Test Book", "Book", "9781234567890");
            
            _mockMediaItemService.Setup(s => s.CreateMediaItemAsync(
                    It.Is<CreateMediaItemCommand>(c => c.UserId == _userId && c.Title == command.Title), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<CreateMediaItemResponse>.Success(response));

            // Act
            var result = await _controller.Create(command);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);
            Assert.Equal(response, createdAtActionResult.Value);
            _mockMediaItemService.Verify(s => s.CreateMediaItemAsync(
                It.Is<CreateMediaItemCommand>(c => c.UserId == _userId && c.Title == command.Title),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_WithExistingId_ShouldReturnOk()
        {
            // Arrange
            var mediaItemId = Guid.NewGuid();
            var response = new GetMediaItemByIdResponse(
                mediaItemId, 
                "Test Book", 
                "Book", 
                2023, 
                "9781234567890", 
                "Description", 
                null, 
                DateTime.Now, 
                null, 
                "Living Room", 
                null, 
                "Test Author");
            
            _mockMediaItemService.Setup(s => s.GetMediaItemByIdAsync(
                    It.Is<GetMediaItemByIdQuery>(q => q.MediaItemId == mediaItemId && q.UserId == _userId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<GetMediaItemByIdResponse>.Success(response));

            // Act
            var result = await _controller.GetById(mediaItemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            _mockMediaItemService.Verify(s => s.GetMediaItemByIdAsync(
                It.Is<GetMediaItemByIdQuery>(q => q.MediaItemId == mediaItemId && q.UserId == _userId),
                It.IsAny<CancellationToken>()), Times.Once);
        }        [Fact]
        public async Task GetById_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var mediaItemId = Guid.NewGuid();
            
            _mockMediaItemService.Setup(s => s.GetMediaItemByIdAsync(
                    It.Is<GetMediaItemByIdQuery>(q => q.MediaItemId == mediaItemId && q.UserId == _userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<GetMediaItemByIdResponse>.Failure("Item not found", OperationErrorType.NotFoundError)); // Return failure to simulate item not found

            // Act
            var result = await _controller.GetById(mediaItemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockMediaItemService.Verify(s => s.GetMediaItemByIdAsync(
                It.Is<GetMediaItemByIdQuery>(q => q.MediaItemId == mediaItemId && q.UserId == _userId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_WithMatchingIds_ShouldReturnNoContent()
        {
            // Arrange
            var mediaItemId = Guid.NewGuid();
            var command = new UpdateMediaItemCommand(
                mediaItemId, 
                Guid.Empty, 
                "Updated Book", 
                "Book", 
                2023, 
                "9781234567890", 
                "Updated Description", 
                null,
                null,
                null);
            var expectedCommand = command with { UserId = _userId };
            
            _mockMediaItemService.Setup(s => s.UpdateMediaItemAsync(
                    It.Is<UpdateMediaItemCommand>(c => c.MediaItemId == mediaItemId && c.UserId == _userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.Update(mediaItemId, command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockMediaItemService.Verify(s => s.UpdateMediaItemAsync(
                It.Is<UpdateMediaItemCommand>(c => c.MediaItemId == mediaItemId && c.UserId == _userId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            var mediaItemId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var command = new UpdateMediaItemCommand(
                differentId, 
                Guid.Empty, 
                "Updated Book", 
                "Book", 
                2023, 
                "9781234567890", 
                "Updated Description", 
                null,
                null,
                null);

            // Act
            var result = await _controller.Update(mediaItemId, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockMediaItemService.Verify(s => s.UpdateMediaItemAsync(
                It.IsAny<UpdateMediaItemCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var mediaItemId = Guid.NewGuid();
            
            _mockMediaItemService.Setup(s => s.DeleteMediaItemAsync(
                    It.Is<DeleteMediaItemCommand>(c => c.MediaItemId == mediaItemId && c.UserId == _userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult.Success());

            // Act
            var result = await _controller.Delete(mediaItemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockMediaItemService.Verify(s => s.DeleteMediaItemAsync(
                It.Is<DeleteMediaItemCommand>(c => c.MediaItemId == mediaItemId && c.UserId == _userId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task List_WithValidQuery_ShouldReturnOkWithResults()
        {
            // Arrange
            var query = new ListMediaItemsQuery(_userId);
            var mediaItems = new List<MediaItemDto> 
            { 
                new MediaItemDto(
                    Guid.NewGuid(), 
                    "Book 1", 
                    "Book", 
                    2023, 
                    "9781234567890", 
                    "Description 1", 
                    null, 
                    DateTime.Now, 
                    null, 
                    "Living Room", 
                    null, 
                    "Test Author"),
                new MediaItemDto(
                    Guid.NewGuid(), 
                    "Book 2", 
                    "Book", 
                    2022, 
                    "9780987654321", 
                    "Description 2", 
                    null, 
                    DateTime.Now, 
                    null, 
                    "Bedroom", 
                    null, 
                    "Another Author")
            };
            var response = new ListMediaItemsResponse(mediaItems, mediaItems.Count, 1, 10);
            
            _mockMediaItemService.Setup(s => s.ListMediaItemsAsync(
                    It.Is<ListMediaItemsQuery>(q => q.UserId == _userId && q.PageNumber == query.PageNumber && q.PageSize == query.PageSize),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(OperationResult<ListMediaItemsResponse>.Success(response));

            // Act
            var result = await _controller.List(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            _mockMediaItemService.Verify(s => s.ListMediaItemsAsync(
                It.Is<ListMediaItemsQuery>(q => q.UserId == _userId && q.PageNumber == query.PageNumber && q.PageSize == query.PageSize),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupUserIdentity(MediaItemsController controller, Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
