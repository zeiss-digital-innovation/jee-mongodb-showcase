# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY DotNetMongoDbBackend/DotNetMongoDbBackend.csproj DotNetMongoDbBackend/
RUN dotnet restore DotNetMongoDbBackend/DotNetMongoDbBackend.csproj

# Copy source code and build
COPY DotNetMongoDbBackend/ DotNetMongoDbBackend/
WORKDIR /src/DotNetMongoDbBackend
RUN dotnet build -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 80

# Start application
ENTRYPOINT ["dotnet", "DotNetMongoDbBackend.dll"]