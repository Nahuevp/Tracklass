# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Tracklass.API.csproj", "./"]
RUN dotnet restore "Tracklass.API.csproj"

# Copy everything and build
COPY . .
RUN dotnet build "Tracklass.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Tracklass.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published output
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Tracklass.API.dll"]
