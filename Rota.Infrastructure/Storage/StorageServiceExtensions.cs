using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rota.Application.Common.Storage;

namespace Rota.Infrastructure.Storage;

public static class StorageServiceExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection services,
                                                IConfiguration cfg)
    {
        services.AddDefaultAWSOptions(cfg.GetAWSOptions());   
        services.AddAWSService<IAmazonS3>();                  

        return services.AddSingleton<IStorageService, S3StorageService>();
    }
}
