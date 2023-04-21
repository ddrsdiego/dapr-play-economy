FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["services/customer/src/Play.Customer.Service/Play.Customer.Service.csproj", "services/customer/src/Play.Customer.Service/"]
COPY ["services/customer/src/Play.Customer.Core/Play.Customer.Core.csproj", "services/customer/src/Play.Customer.Core/"]
COPY ["services/common/src/Play.Common/Play.Common.csproj", "services/common/src/Play.Common/"]
RUN dotnet restore "services/customer/src/Play.Customer.Service/Play.Customer.Service.csproj"
COPY . .
WORKDIR "/src/services/customer/src/Play.Customer.Service"
RUN dotnet build "Play.Customer.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Play.Customer.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Play.Customer.Service.dll"]
