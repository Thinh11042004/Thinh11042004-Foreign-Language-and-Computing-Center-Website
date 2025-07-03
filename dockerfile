# -----------------------
# 1. BASE IMAGE (.NET 8 Runtime)
# -----------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# -----------------------
# 2. SDK IMAGE TO BUILD (.NET 8 SDK)
# -----------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Foreign-language-and-information-technologya-center-teaching-system.csproj", "./"]
RUN dotnet restore "./Foreign-language-and-information-technologya-center-teaching-system.csproj"

# Copy all source and build
COPY . .
RUN dotnet build "./Foreign-language-and-information-technologya-center-teaching-system.csproj" -c Release -o /app/build

# -----------------------
# 3. PUBLISH
# -----------------------
FROM build AS publish
RUN dotnet publish "./Foreign-language-and-information-technologya-center-teaching-system.csproj" -c Release -o /app/publish

# -----------------------
# 4. FINAL RUNTIME IMAGE
# -----------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Foreign-language-and-information-technologya-center-teaching-system.dll"]
