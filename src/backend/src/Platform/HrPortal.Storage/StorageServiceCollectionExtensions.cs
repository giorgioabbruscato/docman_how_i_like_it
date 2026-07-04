using HrPortal.Storage.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IStorageProvider, FileSystemStorageProvider>();
        return services;
    }
}
