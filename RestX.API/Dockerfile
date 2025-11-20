# Dùng .NET 8 SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore (để tận dụng cache)
COPY ["RestX.API/RestX.API.csproj", "RestX.API/"]
RUN dotnet restore "RestX.API/RestX.API.csproj"

# Copy toàn bộ code và build
COPY ["RestX.API/", "RestX.API/"]
WORKDIR "/src/RestX.API"
RUN dotnet build "RestX.API.csproj" -c Release -o /app/build

# Publish ra file DLL
FROM build AS publish
RUN dotnet publish "RestX.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Dùng Runtime để chạy (nhẹ hơn SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestX.API.dll"]