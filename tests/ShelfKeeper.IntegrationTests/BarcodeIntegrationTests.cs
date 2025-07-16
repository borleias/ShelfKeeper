// <copyright file="BarcodeIntegrationTests.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net.Http.Json;
using System.Net;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Application.Services.MediaItems.Models;
using ShelfKeeper.Application.Interfaces;
using Moq;

namespace ShelfKeeper.IntegrationTests
{
    public class BarcodeIntegrationTests : IntegrationTestBase
    {
        public BarcodeIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task ScanBarcode_ShouldCreateMediaItem()
        {
            // Arrange
            await ResetDatabaseAsync();
            var user = await RegisterAndLoginUser("barcodescan@example.com", "Password123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            var barcode = "1234567890123";
            var expectedCommand = new CreateMediaItemCommand
            (
                UserId: user.UserId,
                Title: "Scanned Book Title",
                Type: "Book",
                Year: 2023,
                IsbnUpc: barcode,
                Notes: "Notes from scan",
                Progress: "",
                LocationId: null,
                AuthorId: null
            );

            _factory.MockBarcodeScannerService
                .Setup(s => s.ScanBarcodeAsync(barcode))
                .ReturnsAsync(expectedCommand);

            // Act
            var response = await _client.GetAsync($"/api/v1/barcode/{barcode}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreateMediaItemResponse>();
            Assert.NotNull(result);
            Assert.Equal(expectedCommand.Title, result.Title);
            Assert.Equal(expectedCommand.IsbnUpc, result.IsbnUpc);

            _factory.MockBarcodeScannerService.Verify(s => s.ScanBarcodeAsync(barcode), Times.Once);
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
