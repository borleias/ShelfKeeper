// <copyright file="IntegrationTestBase.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ShelfKeeper.Infrastructure.Persistence;
using System.Net.Http.Json;
using ShelfKeeper.Application.Services.Users.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace ShelfKeeper.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        protected async Task<LoginUserResponse> AuthenticateAsAdminAsync()
        {
            var loginCommand = new LoginUserQuery("admin@example.com", "password123"); // Use your admin credentials
            var response = await _client.PostAsJsonAsync("/api/v1/users/login", loginCommand);
            response.EnsureSuccessStatusCode();
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginUserResponse>();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            return loginResponse;
        }

        protected async Task<LoginUserResponse> AuthenticateAsUserAsync(string email, string password)
        {
            var loginCommand = new LoginUserQuery(email, password);
            var response = await _client.PostAsJsonAsync("/api/v1/users/login", loginCommand);
            response.EnsureSuccessStatusCode();
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginUserResponse>();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            return loginResponse;
        }

        protected async Task ResetDatabaseAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.MigrateAsync();
                // Optionally seed data here if needed for every test
            }
        }
    }
}
