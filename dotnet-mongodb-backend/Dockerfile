# Dockerfile für .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY DotNetMongoDbBackend/DotNetMongoDbBackend.csproj DotNetMongoDbBackend/
RUN dotnet restore DotNetMongoDbBackend/DotNetMongoDbBackend.csproj

# Copy source code and publish
COPY DotNetMongoDbBackend/ DotNetMongoDbBackend/
WORKDIR /src/DotNetMongoDbBackend
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "DotNetMongoDbBackend.dll"]