from mcr.microsoft.com/dotnet/aspnet:6.0

COPY NupkgDownloader.WebApp/bin/Release/net6.0/publish/ App

ENTRYPOINT [ "dotnet", "App/NupkgDownloader.WebApp.dll" ]