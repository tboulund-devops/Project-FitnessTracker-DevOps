# Use .NET 10.0 SDK (preview)
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files first for better restore caching across source edits.
COPY ["Backend/Backend.csproj", "Backend/"]
COPY ["FeaturehubHelper/FeaturehubHelper.csproj", "FeaturehubHelper/"]
RUN dotnet restore "Backend/Backend.csproj"

# Copy source and publish backend.
COPY Backend/. Backend/
COPY FeaturehubHelper/. FeaturehubHelper/
WORKDIR /src/Backend
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish

# Runtime image - also using .NET 10.0 preview
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Backend.dll"]