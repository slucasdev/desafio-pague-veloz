# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/SL.DesafioPagueVeloz.Api/SL.DesafioPagueVeloz.Api.csproj", "src/SL.DesafioPagueVeloz.Api/"]
COPY ["src/SL.DesafioPagueVeloz.Application/SL.DesafioPagueVeloz.Application.csproj", "src/SL.DesafioPagueVeloz.Application/"]
COPY ["src/SL.DesafioPagueVeloz.Domain/SL.DesafioPagueVeloz.Domain.csproj", "src/SL.DesafioPagueVeloz.Domain/"]
COPY ["src/SL.DesafioPagueVeloz.Infrastructure/SL.DesafioPagueVeloz.Infrastructure.csproj", "src/SL.DesafioPagueVeloz.Infrastructure/"]

RUN dotnet restore "src/SL.DesafioPagueVeloz.Api/SL.DesafioPagueVeloz.Api.csproj"

# Copiar todo o código fonte
COPY . .

# Build e Publish
WORKDIR "/src/src/SL.DesafioPagueVeloz.Api"
RUN dotnet build "SL.DesafioPagueVeloz.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SL.DesafioPagueVeloz.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Criar usuário não-root
RUN addgroup --system --gid 1001 appuser && adduser --system --uid 1001 --gid 1001 appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SL.DesafioPagueVeloz.Api.dll"]