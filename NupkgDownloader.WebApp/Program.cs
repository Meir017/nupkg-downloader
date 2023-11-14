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

app.MapGet("/get-download-link", async (string packageId, string packageVersion) =>
{
    return Results.Redirect(BuildNupkgDownloadLink(packageId, packageVersion));
});
app.MapGet("/get-download-links", async (string packageId, string packageVersion, [FromServices] NupkgFetcher nupkgFetcher, [FromServices] IMemoryCache cache) =>
{
    var cacheKey = $"{packageId}_{packageVersion}";
    if (!cache.TryGetValue<IEnumerable<NuGetProjectAction>>(cacheKey, out var packages)) {
        var package = new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion));
        packages = await nupkgFetcher.GetPackagesDependenciesAsync(package, SourceRepositoryCreator.NugetGallery);
        cache.Set(cacheKey, packages.ToArray(), TimeSpan.FromDays(1));
    }

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