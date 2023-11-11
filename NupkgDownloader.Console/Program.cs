using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NupkgDownloader.Core;

if (args.Length != 2)
{
    Console.WriteLine("Usage: NupkgDownloader <packageId> <packageVersion>");
    Environment.Exit(1);
}

var packageId = args[0];
var packageVersion = args[1];
var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));

var services = new ServiceCollection()
    .AddNupkgFetcher()
    .AddSingleton<ILogger, ConsoleLogger>()
    .BuildServiceProvider();

var nugetFetcher = services.GetRequiredService<NupkgFetcher>();

var packages = await nugetFetcher.GetPackagesDependenciesAsync(package, SourceRepositoryCreator.NugetGallery);

await nugetFetcher.DownloadNupkgAsync(packages);
