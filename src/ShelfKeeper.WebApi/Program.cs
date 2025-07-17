// <copyright file="Program.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Infrastructure.Persistence;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.MediaItems;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ShelfKeeper.Application.Services.FeatureGates;
using ShelfKeeper.Application.Services.Subscriptions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>(provider =>
    new AdminUserService(provider.GetRequiredService<IApplicationDbContext>(), provider.GetRequiredService<IPasswordHasher>()));
builder.Services.AddScoped<IMediaItemService, MediaItemService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>(provider =>
    new SubscriptionService(provider.GetRequiredService<IApplicationDbContext>(), provider.GetRequiredService<IStripeService>()));
builder.Services.AddScoped<IFeatureGateService, FeatureGateService>(provider =>
    new FeatureGateService(provider.GetRequiredService<ISubscriptionService>(), provider.GetRequiredService<IApplicationDbContext>()));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());

// Ensure the database is created and seeded on startup
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

// Ensure the database is created and seeded on startup
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBarcodeScannerService, BarcodeScannerService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IStripeService, StripeService>();
builder.Services.AddScoped<IEmailService, DummyEmailService>();
builder.Services.Configure<SubscriptionCheckerSettings>(builder.Configuration.GetSection("SubscriptionCheckerSettings"));

// Only add the SubscriptionDowngradeChecker if not running from the EF Core tools
if (!args.Any(a => a.Contains("ef")))
{
    builder.Services.AddHostedService<SubscriptionDowngradeChecker>();
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: builder.Environment.ApplicationName,
        serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddSource(builder.Environment.ApplicationName);
        tracing.AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddPrometheusExporter();
    });

builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromSeconds(30);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
        Log.Warning("Rate limit exceeded for {RemoteIpAddress}", context.HttpContext.Connection.RemoteIpAddress);
    };
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // Add Serilog for request logging

app.UseHttpsRedirection();

// Global OperationError Handling Middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An unhandled exception occurred: {Message}", ex.Message);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An unexpected error occurred.");
    }
});

app.UseRateLimiter(); // Apply rate limiting

app.UseAuthentication(); // Enable authentication
app.UseAuthorization();

app.MapControllers();

// Map Health Checks and Prometheus Metrics
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint("/metrics");

app.Run();