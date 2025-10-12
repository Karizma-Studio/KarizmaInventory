using KarizmaPlatform.Inventory.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Inventory.Infrastructure;

public class InventoryDatabaseUtilities
{
    public static void ConfigureDatabase<T>(ModelBuilder modelBuilder) where T : class, IInventoryUser
    {
        modelBuilder.Entity<UserInventoryItem>()
            .HasOne<T>()
            .WithMany()
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserInventoryItem>().Ignore(ui => ui.User);


        modelBuilder.Entity<UserInventoryItem>()
            .HasIndex(x => new { x.UserId })
            .HasFilter("deleted_at IS NULL");

        modelBuilder.Entity<UserInventoryItem>()
            .HasIndex(x => new { x.UserId, x.InventoryItemId })
            .HasFilter("deleted_at IS NULL");

        var entities = new[] { typeof(InventoryItem), typeof(UserInventoryItem) };
        
        foreach (var entityType in entities)
        {
            modelBuilder.Entity(entityType)
                .Property("CreatedDate")
                .HasDefaultValueSql("now()");

            modelBuilder.Entity(entityType)
                .Property("UpdatedDate")
                .HasDefaultValueSql("now()");
        }
    }
}

