# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY Blazor_Horizontal_Virtualize/*.csproj ./Blazor_Horizontal_Virtualize/

# Restore dependencies
RUN dotnet restore

# Copy all project files and publish
COPY Blazor_Horizontal_Virtualize/. ./Blazor_Horizontal_Virtualize/
WORKDIR /app/Blazor_Horizontal_Virtualize
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Serve the app with Kestrel
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

# Expose the port
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "Blazor_Horizontal_Virtualize.dll"]