using KarizmaPlatform.Inventory.Application.Processors;
using KarizmaPlatform.Inventory.Application.Processors.Interfaces;
using KarizmaPlatform.Inventory.Infrastructure;
using KarizmaPlatform.Inventory.Infrastructure.Repositories;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KarizmaPlatform.Inventory.Application.Extensions;

public static class BuilderExtensions
{
    /// <summary>
    /// Registers the inventory module.
    /// <typeparamref name="TPrice"/> and <typeparamref name="TMetadata"/> are the classes the
    /// jsonb "price" and "metadata" columns of inventory_items are deserialized into, so any extra
    /// field can be added to an inventory item without changing this package.
    /// </summary>
    public static IServiceCollection AddKarizmaInventory<TEnum, TPrice, TMetadata, TDatabase>
        (this IServiceCollection services)
        where TEnum : struct, Enum
        where TDatabase : IInventoryDatabase
    {
        services
            .AddScoped<IInventoryItemRepository, InventoryItemRepository>()
            .AddScoped<IUserInventoryItemRepository, UserInventoryItemRepository>()
            .AddScoped<IInventoryProcessor<TEnum, TPrice, TMetadata>, InventoryProcessor<TEnum, TPrice, TMetadata>>()
            .AddScoped<IInventoryDatabase>(provider => provider.GetRequiredService<TDatabase>());

        return services;
    }
}
