FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["CncTooling.Api/CncTooling.Api.csproj", "CncTooling.Api/"]
COPY ["CncTooling.Application/CncTooling.Application.csproj", "CncTooling.Application/"]
COPY ["CncTooling.Domain/CncTooling.Domain.csproj", "CncTooling.Domain/"]
COPY ["CncTooling.Infrastructure/CncTooling.Infrastructure.csproj", "CncTooling.Infrastructure/"]
RUN dotnet restore "CncTooling.Api/CncTooling.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CncTooling.Api"
RUN dotnet build "CncTooling.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CncTooling.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CncTooling.Api.dll"]
