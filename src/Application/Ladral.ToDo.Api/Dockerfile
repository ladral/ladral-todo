FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Application/Ladral.ToDo.Api/Ladral.ToDo.Api.csproj", "Application/Ladral.ToDo.Api/"]
COPY ["Common/Ladral.ToDo.Configuration/Ladral.ToDo.Configuration.csproj", "Common/Ladral.ToDo.Configuration/"]
RUN dotnet restore "Application/Ladral.ToDo.Api/Ladral.ToDo.Api.csproj"
COPY . .
WORKDIR "/src/Application/Ladral.ToDo.Api"
RUN dotnet build "Ladral.ToDo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ladral.ToDo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ladral.ToDo.Api.dll"]
