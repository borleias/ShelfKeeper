// <copyright file="CustomWebApplicationFactory.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using ShelfKeeper.Infrastructure.Persistence;
using ShelfKeeper.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IO;
using Moq;

namespace ShelfKeeper.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public Mock<IBarcodeScannerService> MockBarcodeScannerService { get; private set; }
        public Mock<IStripeService> MockStripeService { get; private set; }
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

        public CustomWebApplicationFactory()
        {
            MockBarcodeScannerService = new Mock<IBarcodeScannerService>();
            MockStripeService = new Mock<IStripeService>();
        }

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true, reloadOnChange: true);
                conf.AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration.
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using Testcontainers PostgreSQL.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString()).UseSnakeCaseNamingConvention();
                });

                services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());

                // Remove the app's IBarcodeScannerService registration.
                ServiceDescriptor? barcodeScannerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBarcodeScannerService));

                if (barcodeScannerDescriptor != null)
                {
                    services.Remove(barcodeScannerDescriptor);
                }

                // Add a mocked IBarcodeScannerService.
                services.AddSingleton(MockBarcodeScannerService.Object);

                // Remove the app's IStripeService registration.
                ServiceDescriptor? stripeServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IStripeService));

                if (stripeServiceDescriptor != null)
                {
                    services.Remove(stripeServiceDescriptor);
                }

                // Add a mocked IStripeService.
                services.AddSingleton(MockStripeService.Object);

                // Build the service provider.
                ServiceProvider sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database contexts
                using (IServiceScope scope = sp.CreateScope())
                {
                    IServiceProvider scopedServices = scope.ServiceProvider;
                    ApplicationDbContext db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // Ensure the database is created and apply migrations.
                    db.Database.Migrate();

                    // Seed the database with test data.
                    // This part needs to be implemented based on your seeding strategy.
                    // For now, we'll just ensure migrations are applied.
                }
            });
        }
    }
}
