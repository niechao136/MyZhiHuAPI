FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyZhiHuAPI/MyZhiHuAPI.csproj", "MyZhiHuAPI/"]
RUN dotnet nuget remove source nuget.org
RUN dotnet nuget add source https://nuget.cdn.azure.cn/v3/index.json -n cn
RUN dotnet restore "MyZhiHuAPI/MyZhiHuAPI.csproj" --disable-parallel
COPY . .
WORKDIR "/src/MyZhiHuAPI"
RUN dotnet build "MyZhiHuAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MyZhiHuAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyZhiHuAPI.dll", "--urls=https://+:8000"]
