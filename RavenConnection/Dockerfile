FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base

#Install other SDK´s
#COPY --from=mcr.microsoft.com/dotnet/sdk:5.0 /usr/share/dotnet/sdk /usr/share/dotnet/sdk
#Install other runtime
#COPY --from=mcr.microsoft.com/dotnet/aspnet:5.0 /usr/share/dotnet/shared /usr/share/dotnet/shared

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RavenConnection.csproj", "./"]
RUN dotnet restore "RavenConnection.csproj"
COPY . .
RUN dotnet build "RavenConnection.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RavenConnection.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet" , "RavenConnection.dll"]
