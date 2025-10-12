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
        
        var enumTypeName = ConvertToSnakeCase(typeof(TEnum).Name);
        
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
        var enumTypeName = ConvertToSnakeCase(typeof(TEnum).Name);
        dataSourceBuilder.MapEnum<TEnum>(enumTypeName);
    }
    
    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }
}

