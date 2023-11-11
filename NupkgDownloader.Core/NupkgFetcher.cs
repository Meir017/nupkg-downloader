using NuGet.Packaging;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;

namespace NupkgDownloader.Core;

public class NupkgFetcher
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly NuGetProject _project;
    private readonly NuGetPackageManager _packageManager;

    private readonly INuGetProjectContext _projectContext;

    public NupkgFetcher(ILogger logger, ISettings settings, NuGetProject project, NuGetPackageManager packageManager)
    {
        _logger = logger;
        _settings = settings;
        _project = project;
        _packageManager = packageManager;

        _projectContext = new ConsoleProjectContext(_logger)
        {
            PackageExtractionContext = new PackageExtractionContext(PackageSaveMode.Nupkg, XmlDocFileSaveMode.None, ClientPolicyContext.GetClientPolicy(_settings, _logger), _logger)
        };
    }

    public Task<IEnumerable<NuGetProjectAction>> GetPackagesDependenciesAsync(PackageIdentity packageIdentity, SourceRepository sourceRepository)
    {
        return _packageManager.PreviewInstallPackageAsync(_project, packageIdentity, new ResolutionContext(), _projectContext, sourceRepository, Array.Empty<SourceRepository>(), CancellationToken.None);
    }

    public Task DownloadNupkgAsync(IEnumerable<NuGetProjectAction> packages)
    {
        return _packageManager.ExecuteNuGetProjectActionsAsync(_project, packages, _projectContext, NullSourceCacheContext.Instance, default);
    }
}
