FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["services/inventory/src/Play.Inventory.Service/Play.Inventory.Service.csproj", "services/inventory/src/Play.Inventory.Service/"]
COPY ["services/common/src/Play.Common/Play.Common.csproj", "services/common/src/Play.Common/"]
COPY ["services/inventory/src/Play.Inventory.Core/Play.Inventory.Core.csproj", "services/inventory/src/Play.Inventory.Core/"]
RUN dotnet restore "services/inventory/src/Play.Inventory.Service/Play.Inventory.Service.csproj"
COPY . .
WORKDIR "/src/services/inventory/src/Play.Inventory.Service"
RUN dotnet build "Play.Inventory.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Play.Inventory.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Play.Inventory.Service.dll"]
