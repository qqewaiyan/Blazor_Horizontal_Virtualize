# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Go into project folder
WORKDIR /app/Blazor_Horizontal_Virtualize

# Restore dependencies
RUN dotnet restore

# Publish
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Serve the app with Kestrel
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "Blazor_Horizontal_Virtualize.dll"]
