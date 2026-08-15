FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/backend/OniBusExpress.Api/OniBusExpress.Api.csproj", "src/backend/OniBusExpress.Api/"]
COPY ["src/backend/OniBusExpress.Application/OniBusExpress.Application.csproj", "src/backend/OniBusExpress.Application/"]
COPY ["src/backend/OniBusExpress.Domain/OniBusExpress.Domain.csproj", "src/backend/OniBusExpress.Domain/"]
COPY ["src/backend/OniBusExpress.Infra/OniBusExpress.Infra.csproj", "src/backend/OniBusExpress.Infra/"]
RUN dotnet restore "src/backend/OniBusExpress.Api/OniBusExpress.Api.csproj"

COPY src/backend/ src/backend/
WORKDIR /src/src/backend/OniBusExpress.Api
RUN dotnet build "OniBusExpress.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OniBusExpress.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OniBusExpress.Api.dll"]
