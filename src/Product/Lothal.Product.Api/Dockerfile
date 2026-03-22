FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["src/Product/Lothal.Product.Api/Lothal.Product.Api.csproj", "src/Product/Lothal.Product.Api/"]
COPY ["src/Product/Lothal.Product.Application/Lothal.Product.Application.csproj", "src/Product/Lothal.Product.Application/"]
COPY ["src/Product/Lothal.Product.Domain/Lothal.Product.Domain.csproj", "src/Product/Lothal.Product.Domain/"]
COPY ["src/Product/Lothal.Product.Infrastructure/Lothal.Product.Infrastructure.csproj", "src/Product/Lothal.Product.Infrastructure/"]
COPY ["src/BuildingBlocks/Lothal.BuildingBlocks/Lothal.BuildingBlocks.csproj", "src/BuildingBlocks/Lothal.BuildingBlocks/"]

# Restore dependencies
RUN dotnet restore "./src/Product/Lothal.Product.Api/Lothal.Product.Api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/Product/Lothal.Product.Api"
RUN dotnet build "./Lothal.Product.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lothal.Product.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lothal.Product.Api.dll"]
