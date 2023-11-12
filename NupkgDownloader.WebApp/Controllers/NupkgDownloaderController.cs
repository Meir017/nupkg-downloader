using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace NupkgDownloader.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class NupkgDownloaderController : ControllerBase
{
    private readonly ILogger<NupkgDownloaderController> _logger;

    public NupkgDownloaderController(ILogger<NupkgDownloaderController> logger)
    {
        _logger = logger;
    }

    [HttpGet(template: "get-download-links", Name = "DisplayDownloadLinks")]
    public IActionResult GetDownloadLinks(string packageId, string packageVersion)
    {
        return Ok();
    }

    [HttpGet(template: "get-download-link", Name = "DisplayDownloadLink")]
    public IActionResult GetDownloadLink(string packageId, string packageVersion)
    {
        return Redirect($"https://www.nuget.org/api/v2/package/{packageId}/{packageVersion}");
    }
}
