FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["services/catalog/src/Play.Catalog.Service/Play.Catalog.Service.csproj", "services/catalog/src/Play.Catalog.Service/"]
COPY ["services/catalog/src/Play.Catalog.Core/Play.Catalog.Core.csproj", "services/catalog/src/Play.Catalog.Core/"]
COPY ["services/common/src/Play.Common/Play.Common.csproj", "services/common/src/Play.Common/"]
RUN dotnet restore "services/catalog/src/Play.Catalog.Service/Play.Catalog.Service.csproj"
COPY . .
WORKDIR "/src/services/catalog/src/Play.Catalog.Service"
RUN dotnet build "Play.Catalog.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Play.Catalog.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Play.Catalog.Service.dll"]
