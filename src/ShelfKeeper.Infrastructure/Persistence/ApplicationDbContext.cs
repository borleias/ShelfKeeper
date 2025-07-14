// <copyright file="ApplicationDbContext.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Infrastructure.Extensions;
using ShelfKeeper.Application.Interfaces;
using EFCore.NamingConventions;

namespace ShelfKeeper.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the database context for the ShelfKeeper application.
    /// </summary>
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="User"/> entities.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="Author"/> entities.
        /// </summary>
        public DbSet<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="Location"/> entities.
        /// </summary>
        public DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="MediaItem"/> entities.
        /// </summary>
        public DbSet<MediaItem> MediaItems { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="MediaImage"/> entities.
        /// </summary>
        public DbSet<MediaImage> MediaImages { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="MediaTag"/> entities.
        /// </summary>
        public DbSet<MediaTag> MediaTags { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="MediaItemTag"/> entities.
        /// </summary>
        public DbSet<MediaItemTag> MediaItemTags { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for <see cref="Subscription"/> entities.
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous save operation. The task operationResult contains the number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types exposed in <see cref="DbSet{TEntity}"/> properties on this context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship for MediaItemTags
            modelBuilder.Entity<MediaItemTag>()
                .HasKey(mit => new { mit.MediaItemId, mit.MediaTagId });

            modelBuilder.Entity<MediaItemTag>()
                .HasOne(mit => mit.MediaItem)
                .WithMany(mi => mi.MediaItemTags)
                .HasForeignKey(mit => mit.MediaItemId);

            modelBuilder.Entity<MediaItemTag>()
                .HasOne(mit => mit.MediaTag)
                .WithMany(mt => mt.MediaItemTags)
                .HasForeignKey(mit => mit.MediaTagId);

            modelBuilder.Entity<MediaItem>()
                .HasMany(mi => mi.MediaImages)
                .WithOne(img => img.MediaItem)
                .HasForeignKey(img => img.MediaItemId);

        }
    }
}