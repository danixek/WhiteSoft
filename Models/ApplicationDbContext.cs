using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WhiteSoft.Models;

namespace WhiteSoft.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>().ToTable("LogEntries");

        // This seeds few products to the database - products are in database from the first start
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Eshop", Type = "Web", Price = 2000, IsPinned = true, ImageUrl = "/img/carousel/wallpaper1.png", MaxCapacity = null },
            new Product { Id = 2, Name = "Firemní stránky", Type = "Web", Price = 2000, IsPinned = true, ImageUrl = "/img/carousel/wallpaper1.png", MaxCapacity = null },
            new Product { Id = 3, Name = "Desktopová aplikace", Type = "Desktop", Price = 3000, ImageUrl = "/img/carousel/wallpaper3.png", MaxCapacity = null },
            new Product { Id = 4, Name = "Mobilní aplikace", Type = "Desktop", Price = 4000, ImageUrl = "/img/carousel/wallpaper1.png", MaxCapacity = null },
            new Product { Id = 5, Name = "Webhosting", Type = "Služby", Price = 1500, ImageUrl = "/img/carousel/wallpaper3.png", MaxCapacity = null },
            new Product { Id = 6, Name = "Oprava počítače", Type = "Služby", Price = 1500, ImageUrl = "/img/carousel/wallpaper2.png", MaxCapacity = 5 }
        );
    }
}