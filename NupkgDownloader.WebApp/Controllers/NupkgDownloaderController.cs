using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NupkgDownloader.Core;

namespace NupkgDownloader.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class NupkgDownloaderController : ControllerBase
{
    private readonly ILogger<NupkgDownloaderController> _logger;
    private readonly NupkgFetcher _nupkgFetcher;

    public NupkgDownloaderController(NupkgFetcher nupkgFetcher, ILogger<NupkgDownloaderController> logger)
    {
        _nupkgFetcher = nupkgFetcher;
        _logger = logger;
    }

    [HttpGet(template: "get-download-links")]
    public async Task<IActionResult> DisplayDownloadLinks(string packageId, string packageVersion)
    {
        _logger.LogInformation("GetDownloadLinks called with packageId: {packageId} and packageVersion: {packageVersion}", packageId, packageVersion);
        var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));
        var packages = await _nupkgFetcher.GetPackagesDependenciesAsync(package, SourceRepositoryCreator.NugetGallery);

        return Ok(packages.Select(x => new
        {
            id = x.PackageIdentity.Id,
            version = x.PackageIdentity.Version.ToNormalizedString(),
            downloadLink = Url.Link(nameof(DisplayDownloadLink), new
            {
                packageId = x.PackageIdentity.Id,
                packageVersion = x.PackageIdentity.Version.ToNormalizedString()
            })
        }));
    }

    [HttpGet(template: "get-download-link", Name = nameof(DisplayDownloadLink))]
    public IActionResult DisplayDownloadLink(string packageId, string packageVersion)
    {
        return Redirect($"https://www.nuget.org/api/v2/package/{packageId}/{packageVersion}");
    }
}
