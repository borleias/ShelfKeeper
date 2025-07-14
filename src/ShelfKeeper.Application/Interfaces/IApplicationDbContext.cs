using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Domain.Entities;

namespace ShelfKeeper.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Author> Authors { get; set; }
        DbSet<Location> Locations { get; set; }
        DbSet<MediaItem> MediaItems { get; set; }
        DbSet<MediaImage> MediaImages { get; set; }
        DbSet<MediaTag> MediaTags { get; set; }
        DbSet<MediaItemTag> MediaItemTags { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
