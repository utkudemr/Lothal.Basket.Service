FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["src/Api/Lothal.Basket.Service.Api.csproj", "src/Api/"]
COPY ["src/Application/Lothal.Basket.Service.Application.csproj", "src/Application/"]
COPY ["src/Domain/Lothal.Basket.Service.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Lothal.Basket.Service.Infrastructure.csproj", "src/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "./src/Api/Lothal.Basket.Service.Api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "./Lothal.Basket.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lothal.Basket.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lothal.Basket.Service.Api.dll"]
