using NupkgDownloader.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddNupkgFetcher();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
