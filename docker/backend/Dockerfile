FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/backend/HrPortal.sln ./
COPY src/backend/Directory.Build.props ./
COPY src/backend/src/ ./src/
COPY src/backend/tests/ ./tests/

RUN dotnet restore HrPortal.sln
RUN dotnet publish src/HrPortal.Api/HrPortal.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/storage

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "HrPortal.Api.dll"]
