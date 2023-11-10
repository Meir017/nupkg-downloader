using NuGet.Packaging;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;

if (args.Length != 2)
{
    Console.WriteLine("Usage: NupkgDownloader <packageId> <packageVersion>");
    Environment.Exit(1);
}

var packageId = args[0];
var packageVersion = args[1];
var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));

ILogger logger = new ConsoleLogger();
ISettings settings = NullSettings.Instance;
var sourceRepository = CreateSourceRepository();

ISourceRepositoryProvider sourceRepositoryProvider = new SourceRepositoryProvider(new PackageSourceProvider(settings), Repository.Provider.GetCoreV3());
var packagesPath = Path.GetFullPath("./packages");
var packageManager = new NuGetPackageManager(sourceRepositoryProvider, settings, packagesPath);

NuGetProject project = new FolderNuGetProject(packagesPath);
var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, logger);
INuGetProjectContext projectContext = new ConsoleProjectContext(logger)
{
    PackageExtractionContext = new PackageExtractionContext(PackageSaveMode.Nuspec, XmlDocFileSaveMode.None, clientPolicyContext, logger)
};

await packageManager.InstallPackageAsync(project, package, new ResolutionContext(), projectContext, sourceRepository, Array.Empty<SourceRepository>(), CancellationToken.None);

SourceRepository CreateSourceRepository()
{
    List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
    providers.AddRange(Repository.Provider.GetCoreV3());

    var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
    return new SourceRepository(packageSource, providers);
}