FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

RUN apt-get update
RUN apt-get install python3 python3-pip -y
RUN pip install mkdocs-material --break-system-packages
RUN apt-get clean

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocmostExporter/DocmostExporter.csproj", "DocmostExporter/"]
RUN dotnet restore "DocmostExporter/DocmostExporter.csproj"
COPY . .
WORKDIR "/src/DocmostExporter"

RUN dotnet build "DocmostExporter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DocmostExporter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY DocmostExporter/entrypoint.sh /app/entrypoint.sh
ENTRYPOINT ["bash", "entrypoint.sh"]
