FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 63023
EXPOSE 44340

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY PoC.API/PoC.API.csproj PoC.API/
COPY PoC.Grains/PoC.Grains.csproj PoC.Grains/
RUN dotnet restore PoC.API/PoC.API.csproj
COPY . .
WORKDIR /src/PoC.API
RUN dotnet build PoC.API.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PoC.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PoC.API.dll"]
