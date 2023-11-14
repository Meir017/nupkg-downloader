using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NupkgDownloader.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNupkgFetcher();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseResponseCaching();

app.MapGet("/get-download-link", async (string packageId, string packageVersion) =>
{
    return Results.Redirect(BuildNupkgDownloadLink(packageId, packageVersion));
});
app.MapGet("/get-download-links", async (string packageId, string packageVersion, [FromServices] NupkgFetcher nupkgFetcher) =>
{
    var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));
    var packages = await nupkgFetcher.GetPackagesDependenciesAsync(package, SourceRepositoryCreator.NugetGallery);

    return packages.Select(x => new
    {
        id = x.PackageIdentity.Id,
        version = x.PackageIdentity.Version.ToNormalizedString(),
        downloadLink = BuildNupkgDownloadLink(x.PackageIdentity.Id, x.PackageIdentity.Version.ToNormalizedString())
    });
});

app.MapFallback(async (HttpContext context) =>
{
    var urlBase = $"https://{context.Request.Host}";
    await context.Response.WriteAsync($@"
    Endpoints: 
    - {urlBase}/get-download-link?packageId=Newtonsoft.Json&packageVersion=12.0.3
    - {urlBase}/get-download-links?packageId=Newtonsoft.Json&packageVersion=12.0.3
    ");
});

app.Run();

static string BuildNupkgDownloadLink(string packageId, string packageVersion)
{
    return $"https://www.nuget.org/api/v2/package/{packageId}/{packageVersion}";
}