FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Application/Ladral.ToDo.WebApp/Ladral.ToDo.WebApp.Bff/Ladral.ToDo.WebApp.Bff.csproj", "Application/Ladral.ToDo.WebApp/Ladral.ToDo.WebApp.Bff/"]
COPY ["Application/Ladral.ToDo.WebApp/Ladral.ToDo.WebApp.Client/Ladral.ToDo.WebApp.Client.csproj", "Application/Ladral.ToDo.WebApp/Ladral.ToDo.WebApp.Client/"]
COPY ["Common/Ladral.ToDo.Configuration/Ladral.ToDo.Configuration.csproj", "Common/Ladral.ToDo.Configuration/"]
RUN dotnet restore "Application/Ladral.ToDo.WebApp/Ladral.ToDo.WebApp.Bff/Ladral.ToDo.WebApp.Bff.csproj"
COPY . .
WORKDIR "/src/Application/Ladral.ToDo.WebApp/Ladral.ToDo.WebApp.Bff"
RUN dotnet build "Ladral.ToDo.WebApp.Bff.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ladral.ToDo.WebApp.Bff.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ladral.ToDo.WebApp.Bff.dll"]
