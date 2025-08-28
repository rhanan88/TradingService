FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /
COPY ["TradingService.Consumer/TradingService.Consumer.csproj", "TradingService.Consumer/"]
COPY ["TradingService.Core/TradingService.Core.csproj", "TradingService.Core/"]
RUN dotnet restore "TradingService.Consumer/TradingService.Consumer.csproj"
COPY . .
WORKDIR "/TradingService.Consumer"
RUN dotnet build "TradingService.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TradingService.Consumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingService.Consumer.dll"]