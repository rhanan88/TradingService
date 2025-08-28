FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /
COPY ["TradingService.Api/TradingService.Api.csproj", "TradingService.Api/"]
COPY ["TradingService.Core/TradingService.Core.csproj", "TradingService.Core/"]
COPY ["TradingService.Infrastructure/TradingService.Infrastructure.csproj", "TradingService.Infrastructure/"]
RUN dotnet restore "TradingService.Api/TradingService.Api.csproj"
COPY . .
WORKDIR "/TradingService.Api"
RUN dotnet build "TradingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TradingService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingService.Api.dll"]