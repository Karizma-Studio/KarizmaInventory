using KarizmaPlatform.Inventory.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace KarizmaPlatform.Inventory.Infrastructure;

public class InventoryDatabaseUtilities
{
    public static void ConfigureDatabase<T, TEnum>(ModelBuilder modelBuilder) 
        where T : class, IInventoryUser
        where TEnum : struct, Enum
    {
        modelBuilder.HasPostgresEnum<TEnum>();
        
        var enumTypeName = typeof(TEnum).Name.ToLower().Replace("type", "_type");
        
        modelBuilder.Entity<InventoryItem>()
            .Property(b => b.Type)
            .HasColumnType(enumTypeName);
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
    
    public static void MapEnums<TEnum>(NpgsqlDataSourceBuilder dataSourceBuilder) where TEnum : struct, Enum
    {
        var enumTypeName = typeof(TEnum).Name.ToLower().Replace("type", "_type");
        dataSourceBuilder.MapEnum<TEnum>(enumTypeName);
    }
}

