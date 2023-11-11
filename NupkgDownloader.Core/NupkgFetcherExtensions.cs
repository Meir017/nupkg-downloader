using Microsoft.Extensions.DependencyInjection;
using NuGet.ProjectManagement;

namespace NupkgDownloader.Core;

public static class NupkgFetcherExtensions
{
    public static IServiceCollection AddNupkgFetcher(this IServiceCollection services)
    {
        services.AddSingleton<ILogger>(NullLogger.Instance);
        services.AddSingleton<ISettings>(NullSettings.Instance);
        services.AddSingleton<IPackageSourceProvider, PackageSourceProvider>();
        services.AddSingleton<ISourceRepositoryProvider>(sp => 
        {
            return new SourceRepositoryProvider(sp.GetRequiredService<IPackageSourceProvider>(), Repository.Provider.GetCoreV3());
        });
        var packagesPath = Path.GetFullPath("./packages");
        services.AddSingleton<NuGetProject>(new FolderNuGetProject(packagesPath));
        services.AddSingleton<NuGetPackageManager>(sp =>
        {
            return new NuGetPackageManager(sp.GetRequiredService<ISourceRepositoryProvider>(), sp.GetRequiredService<ISettings>(), packagesPath);
        });
        services.AddSingleton<NupkgFetcher>();
        return services;
    }
}