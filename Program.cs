using System.Text.Json;

if (args.Length != 2)
{
    Console.WriteLine("Usage: NupkgDownloader <packageId> <packageVersion>");
    Environment.Exit(1);
}

var packageId = args[0];
var packageVersion = args[1];
var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));

ILogger logger = new ConsoleLogger();
SourceCacheContext sourceCache = NullSourceCacheContext.Instance;
var sourceRepository = CreateSourceRepository();

var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
var searchMetadata = await packageMetadataResource.GetMetadataAsync(packageId, true, true, sourceCache, logger, default);

var packageMetadata = searchMetadata.FirstOrDefault(m => m.Identity.Version.ToString() == packageVersion);
if (packageMetadata == null)
{
    Console.WriteLine($"Package {packageId} {packageVersion} not found");
    Console.WriteLine("Available versions:");
    Console.WriteLine("\t" + string.Join(",\n\t", searchMetadata.Select(m => m.Identity.Version.ToString())));
    Environment.Exit(1);
}

Console.WriteLine($"{packageMetadata.Identity.Id} {packageMetadata.Identity.Version} - {packageMetadata.ToJson()}");

var dependencies = (await ResolverGather.GatherAsync(new GatherContext
{
    PrimaryTargets = new[] { package },
    InstalledPackages = Array.Empty<SourcePackageDependencyInfo>(),
    TargetFramework = NuGetFramework.Parse("net6.0"),
    PrimarySources = new[] { sourceRepository },
    AllSources = new[] { sourceRepository },
    PackagesFolderSource = sourceRepository,
    ResolutionContext = new(DependencyBehavior.HighestMinor, false, false, VersionConstraints.ExactRelease),
}, default)).ToArray();

var latestMajorVersionDependencies = dependencies
    .Where(d => !d.Version.IsPrerelease && d.Listed)
    .GroupBy(d => d.Id)
    .SelectMany(g =>
    {
        var highestMajors = g.GroupBy(x => x.Version.Major);
        return g.GroupBy(x => x.Version.Major).Select(x => x.MaxBy(x => x.Version));
    })
    .ToArray();

Console.WriteLine($"Found {dependencies.Length} dependencies");
Console.WriteLine($"Found {latestMajorVersionDependencies.Length} latest major version dependencies");

var jsonOutput = JsonSerializer.Serialize(latestMajorVersionDependencies.Select(x => new
{
    id = x.Id,
    version = x.Version.ToString(),
    downloadUri = x.DownloadUri
}), new JsonSerializerOptions
{
    WriteIndented = true
});
await File.WriteAllTextAsync($"{packageId}-{packageVersion}-dependencies.json", jsonOutput);

return;

SourceRepository CreateSourceRepository()
{
    List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
    providers.AddRange(Repository.Provider.GetCoreV3());

    var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
    return new SourceRepository(packageSource, providers);
}