########################################
# Stage 1 — Build
########################################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore & publish
RUN dotnet restore HorizontalVirtualizationComponent.slnx
RUN dotnet publish HorizontalVirtualizationComponent.slnx -c Release -o /app/publish

########################################
# Stage 2 — Serve
########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "HorizontalVirtualizationComponent.dll"]
