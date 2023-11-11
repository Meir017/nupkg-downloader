namespace NupkgDownloader.Core;

public static class SourceRepositoryCreator
{
    public static SourceRepository NugetGallery { get; }

    static SourceRepositoryCreator()
    {
        List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
        providers.AddRange(Repository.Provider.GetCoreV3());

        var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
        NugetGallery = new SourceRepository(packageSource, providers);
    }
}
