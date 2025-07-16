// <copyright file="MediaItemIntegrationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net.Http.Json;
using System.Net;
using ShelfKeeper.Application.Services.MediaItems.Models;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ShelfKeeper.IntegrationTests
{
    public class MediaItemIntegrationTests : IntegrationTestBase
    {
        public MediaItemIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateMediaItem_ShouldReturnSuccess()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("mediauser@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var command = new CreateMediaItemCommand
            (
                UserId: user.UserId,
                Title: "Test Book",
                Type: "Book",
                Year: 2023,
                IsbnUpc: "1234567890",
                Notes: "Some notes",
                Progress: "0",
                LocationId: null,
                AuthorId: null
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/mediaitems", command);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreateMediaItemResponse>();
            Assert.NotNull(result);
            Assert.Equal(command.Title, result.Title);
        }

        [Fact]
        public async Task GetMediaItemById_ShouldReturnMediaItem()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("getmedia@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var createCommand = new CreateMediaItemCommand
            (
                UserId: user.UserId,
                Title: "Another Book",
                Type: "Book",
                Year: 2022,
                IsbnUpc: "0987654321",
                Notes: "",
                Progress: "",
                LocationId: null,
                AuthorId: null
            );
            var createResponse = await _client.PostAsJsonAsync("/api/v1/mediaitems", createCommand);
            createResponse.EnsureSuccessStatusCode();
            var createdMediaItem = await createResponse.Content.ReadFromJsonAsync<CreateMediaItemResponse>();

            // Act
            var response = await _client.GetAsync($"/api/v1/mediaitems/{createdMediaItem.MediaItemId}?userId={user.UserId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var mediaItem = await response.Content.ReadFromJsonAsync<GetMediaItemByIdResponse>();
            Assert.NotNull(mediaItem);
            Assert.Equal(createdMediaItem.MediaItemId, mediaItem.MediaItemId);
            Assert.Equal(createCommand.Title, mediaItem.Title);
        }

        [Fact]
        public async Task GetMediaItemById_OtherUserMedia_ShouldReturnNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user1 = await RegisterAndLoginUser("user1@example.com", "Password123!");
            var user2 = await RegisterAndLoginUser("user2@example.com", "Password123!");

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user1.Token);
            var createCommand = new CreateMediaItemCommand
            (
                UserId: user1.UserId,
                Title: "User1's Book",
                Type: "Book",
                Year: 2020,
                IsbnUpc: "1111111111",
                Notes: "",
                Progress: "",
                LocationId: null,
                AuthorId: null
            );
            var createResponse = await _client.PostAsJsonAsync("/api/v1/mediaitems", createCommand);
            createResponse.EnsureSuccessStatusCode();
            var createdMediaItem = await createResponse.Content.ReadFromJsonAsync<CreateMediaItemResponse>();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user2.Token);

            // Act
            var response = await _client.GetAsync($"/api/v1/mediaitems/{createdMediaItem.MediaItemId}?userId={user2.UserId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMediaItem_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("updatemedia@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var createCommand = new CreateMediaItemCommand
            (
                UserId: user.UserId,
                Title: "Original Title",
                Type: "Book",
                Year: 2021,
                IsbnUpc: "2222222222",
                Notes: "",
                Progress: "",
                LocationId: null,
                AuthorId: null
            );
            var createResponse = await _client.PostAsJsonAsync("/api/v1/mediaitems", createCommand);
            createResponse.EnsureSuccessStatusCode();
            var createdMediaItem = await createResponse.Content.ReadFromJsonAsync<CreateMediaItemResponse>();

            var updateCommand = new UpdateMediaItemCommand
            (
                MediaItemId: createdMediaItem.MediaItemId,
                UserId: user.UserId,
                Title: "Updated Title",
                Type: "Book",
                Year: 2021,
                IsbnUpc: "2222222222",
                Notes: "Updated notes",
                Progress: "100",
                LocationId: null,
                AuthorId: null
            );

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/mediaitems/{createdMediaItem.MediaItemId}", updateCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify update
            var getResponse = await _client.GetAsync($"/api/v1/mediaitems/{createdMediaItem.MediaItemId}?userId={user.UserId}");
            getResponse.EnsureSuccessStatusCode();
            var updatedMediaItem = await getResponse.Content.ReadFromJsonAsync<GetMediaItemByIdResponse>();
            Assert.Equal("Updated Title", updatedMediaItem.Title);
            Assert.Equal("Updated notes", updatedMediaItem.Notes);
        }

        [Fact]
        public async Task DeleteMediaItem_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("deletemedia@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var createCommand = new CreateMediaItemCommand
            (
                UserId: user.UserId,
                Title: "To Be Deleted",
                Type: "Book",
                Year: 2024,
                IsbnUpc: "3333333333",
                Notes: "",
                Progress: "",
                LocationId: null,
                AuthorId: null
            );
            var createResponse = await _client.PostAsJsonAsync("/api/v1/mediaitems", createCommand);
            createResponse.EnsureSuccessStatusCode();
            var createdMediaItem = await createResponse.Content.ReadFromJsonAsync<CreateMediaItemResponse>();

            // Act
            var response = await _client.DeleteAsync($"/api/v1/mediaitems/{createdMediaItem.MediaItemId}?userId={user.UserId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/v1/mediaitems/{createdMediaItem.MediaItemId}?userId={user.UserId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ListMediaItems_ShouldReturnFilteredAndPaginatedList()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("listmedia@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            // Create multiple media items for testing list, filter, search, pagination
            await _client.PostAsJsonAsync("/api/v1/mediaitems", new CreateMediaItemCommand(user.UserId, "Book A", "Book", 2000, "", "", "", null, null));
            await _client.PostAsJsonAsync("/api/v1/mediaitems", new CreateMediaItemCommand(user.UserId, "Book B", "Book", 2001, "", "", "", null, null));
            await _client.PostAsJsonAsync("/api/v1/mediaitems", new CreateMediaItemCommand(user.UserId, "Movie C", "Movie", 2002, "", "", "", null, null));
            await _client.PostAsJsonAsync("/api/v1/mediaitems", new CreateMediaItemCommand(user.UserId, "Book D", "Book", 2003, "", "", "", null, null));

            // Act: List all
            var responseAll = await _client.GetAsync($"/api/v1/mediaitems?userId={user.UserId}");
            responseAll.EnsureSuccessStatusCode();
            var listAll = await responseAll.Content.ReadFromJsonAsync<ListMediaItemsResponse>();
            Assert.Equal(4, listAll.TotalCount);

            // Act: Filter by type
            var responseFiltered = await _client.GetAsync($"/api/v1/mediaitems?userId={user.UserId}&typeFilter=Book");
            responseFiltered.EnsureSuccessStatusCode();
            var listFiltered = await responseFiltered.Content.ReadFromJsonAsync<ListMediaItemsResponse>();
            Assert.Equal(3, listFiltered.TotalCount);

            // Act: Search by title
            var responseSearch = await _client.GetAsync($"/api/v1/mediaitems?userId={user.UserId}&searchTerm=Movie");
            responseSearch.EnsureSuccessStatusCode();
            var listSearch = await responseSearch.Content.ReadFromJsonAsync<ListMediaItemsResponse>();
            Assert.Equal(1, listSearch.TotalCount);
            Assert.Equal("Movie C", listSearch.MediaItems.First().Title);

            // Act: Pagination
            var responsePaginated = await _client.GetAsync($"/api/v1/mediaitems?userId={user.UserId}&pageNumber=1&pageSize=2");
            responsePaginated.EnsureSuccessStatusCode();
            var listPaginated = await responsePaginated.Content.ReadFromJsonAsync<ListMediaItemsResponse>();
            Assert.Equal(4, listPaginated.TotalCount);
            Assert.Equal(2, listPaginated.MediaItems.Count);
        }

        private async Task<LoginUserResponse> RegisterAndLoginUser(string email, string password)
        {
            var registerCommand = new CreateUserCommand(email, password, "Test User");
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/users/register", registerCommand);
            registerResponse.EnsureSuccessStatusCode();

            var loginQuery = new LoginUserQuery(email, password);
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginQuery);
            loginResponse.EnsureSuccessStatusCode();
            return await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        }
    }
}
