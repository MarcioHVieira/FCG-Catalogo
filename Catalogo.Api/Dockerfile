FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

RUN apk add --no-cache icu-libs=76.1-r1
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY nuget.config ./
COPY packages ./packages

COPY Catalogo.Api/Catalogo.Api.csproj ./Catalogo.Api/
RUN dotnet restore ./Catalogo.Api/Catalogo.Api.csproj

COPY Catalogo.Api/. ./Catalogo.Api/

RUN dotnet publish ./Catalogo.Api/Catalogo.Api.csproj -c Release -o /app/publish --no-restore --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs=76.1-r1
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Catalogo.Api.dll"]