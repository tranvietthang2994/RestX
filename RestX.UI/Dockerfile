# Dùng .NET 8 SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore
COPY ["RestX.UI/RestX.UI.csproj", "RestX.UI/"]
RUN dotnet restore "RestX.UI/RestX.UI.csproj"

# Copy toàn bộ code và build
COPY ["RestX.UI/", "RestX.UI/"]
WORKDIR "/src/RestX.UI"
RUN dotnet build "RestX.UI.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "RestX.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestX.UI.dll"]