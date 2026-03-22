FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["src/Basket/Lothal.Basket.Api/Lothal.Basket.Api.csproj", "src/Basket/Lothal.Basket.Api/"]
COPY ["src/Basket/Lothal.Basket.Application/Lothal.Basket.Application.csproj", "src/Basket/Lothal.Basket.Application/"]
COPY ["src/Basket/Lothal.Basket.Domain/Lothal.Basket.Domain.csproj", "src/Basket/Lothal.Basket.Domain/"]
COPY ["src/Basket/Lothal.Basket.Infrastructure/Lothal.Basket.Infrastructure.csproj", "src/Basket/Lothal.Basket.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "./src/Basket/Lothal.Basket.Api/Lothal.Basket.Api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/Basket/Lothal.Basket.Api"
RUN dotnet build "./Lothal.Basket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lothal.Basket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lothal.Basket.Api.dll"]
