using KarizmaPlatform.Inventory.Application.Processors;
using KarizmaPlatform.Inventory.Application.Processors.Interfaces;
using KarizmaPlatform.Inventory.Infrastructure;
using KarizmaPlatform.Inventory.Infrastructure.Repositories;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KarizmaPlatform.Inventory.Application.Extensions;

public static class BuilderExtensions
{
    public static IServiceCollection AddKarizmaInventory<TEnum, TPrice, TDatabase>
        (this IServiceCollection services) 
        where TEnum : struct, Enum 
        where TDatabase : IInventoryDatabase
    {
        services
            .AddScoped<IInventoryItemRepository, InventoryItemRepository>()
            .AddScoped<IUserInventoryItemRepository, UserInventoryItemRepository>()
            .AddScoped<IInventoryProcessor<TEnum, TPrice>, InventoryProcessor<TEnum, TPrice>>()
            .AddScoped<IInventoryDatabase>(provider => provider.GetRequiredService<TDatabase>());

        return services;
    }
}


