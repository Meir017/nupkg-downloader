using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NupkgDownloader.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNupkgFetcher();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();

app.Use((context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(1)
    };
    return next();
});

app.MapGet("/get-download-link", (string packageId, string packageVersion) => Results.Redirect(BuildNupkgDownloadLink(packageId, packageVersion)));
app.MapGet("/get-download-links", async (string packageId, string packageVersion, HttpContext context, [FromServices] NupkgFetcher nupkgFetcher, [FromServices] IMemoryCache cache) =>
{
    var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));
    var cacheKey = $"{packageId}_{packageVersion}";
    if (!cache.TryGetValue<IEnumerable<NuGetProjectAction>>(cacheKey, out var packages))
    {
        packages = await nupkgFetcher.GetPackagesDependenciesAsync(package, SourceRepositoryCreator.NugetGallery);
        cache.Set(cacheKey, packages.ToArray(), TimeSpan.FromDays(1));
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync($"""
        <h1>Download links for {packageId} {packageVersion} dependencies</h1>
        <ul>
            {string.Join("\n", packages.Select(x => $"<li><a href=\"{BuildNupkgDownloadLink(x.PackageIdentity.Id, x.PackageIdentity.Version.ToNormalizedString())}\">{x.PackageIdentity.Id} {x.PackageIdentity.Version.ToNormalizedString()}</a></li>"))}
        </ul>
    """);
});

app.MapFallback(async (HttpContext context) =>
{
    var urlBase = $"https://{context.Request.Host}";
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(
        $"""
            <h1>Nupkg Downloader</h1>
            
            <p>Get download link for a package</p> <a href="{urlBase}/get-download-link?packageId=Newtonsoft.Json&amp;packageVersion=12.0.3">/get-download-link?packageId=Newtonsoft.Json&amp;packageVersion=12.0.3</a>
            <p>Get download links for a package and its dependencies</p> <a href="{urlBase}/get-download-links?packageId=Newtonsoft.Json&amp;packageVersion=12.0.3">/get-download-links?packageId=Newtonsoft.Json&amp;packageVersion=12.0.3</a>
        """);
});

app.Run();

static string BuildNupkgDownloadLink(string packageId, string packageVersion)
{
    return $"https://www.nuget.org/api/v2/package/{packageId}/{packageVersion}";
}