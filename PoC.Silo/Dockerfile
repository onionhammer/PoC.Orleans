FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY PoC.Silo/PoC.Silo.csproj PoC.Silo/
COPY PoC.Grains/PoC.Grains.csproj PoC.Grains/
COPY PoC.Grains.Implementation/PoC.Grains.Implementation.csproj PoC.Grains.Implementation/
RUN dotnet restore PoC.Silo/PoC.Silo.csproj
COPY . .
WORKDIR /src/PoC.Silo
RUN dotnet build PoC.Silo.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PoC.Silo.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PoC.Silo.dll"]
