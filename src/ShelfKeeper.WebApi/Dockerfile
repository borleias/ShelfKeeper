FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/ShelfKeeper.WebApi/ShelfKeeper.WebApi.csproj", "src/ShelfKeeper.WebApi/"]
COPY ["src/ShelfKeeper.Application/ShelfKeeper.Application.csproj", "src/ShelfKeeper.Application/"]
COPY ["src/ShelfKeeper.Domain/ShelfKeeper.Domain.csproj", "src/ShelfKeeper.Domain/"]
COPY ["src/ShelfKeeper.Infrastructure/ShelfKeeper.Infrastructure.csproj", "src/ShelfKeeper.Infrastructure/"]
RUN dotnet restore "src/ShelfKeeper.WebApi/ShelfKeeper.WebApi.csproj"

COPY . .
WORKDIR "/src/src/ShelfKeeper.WebApi"
RUN dotnet build "ShelfKeeper.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShelfKeeper.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShelfKeeper.WebApi.dll"]
