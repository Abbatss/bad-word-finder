#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["BWF.Api.Host/BWF.Api.Host.csproj", "BWF.Api.Host/"]
COPY ["DynamoDB.Common/DynamoDB.Common/DynamoDB.Common.csproj", "DynamoDB.Common/DynamoDB.Common/"]
COPY ["Api.Common/Api.Common.csproj", "Api.Common/"]
RUN dotnet restore "BWF.Api.Host/BWF.Api.Host.csproj"
COPY . .
WORKDIR "/src/BWF.Api.Host"
RUN dotnet build "BWF.Api.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BWF.Api.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BWF.Api.Host.dll"]
