# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all .csproj files and restore dependencies
COPY ["Bridgette.Api/Bridgette.Api.csproj", "Bridgette.Api/"]
COPY ["Bridgette.Core/Bridgette.Core.csproj", "Bridgette.Core/"]
COPY ["Bridgette.Data/Bridgette.Data.csproj", "Bridgette.Data/"]
COPY ["Bridgette.Google/Bridgette.Google.csproj", "Bridgette.Google/"]
RUN dotnet restore "Bridgette.Api/Bridgette.Api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/Bridgette.Api"
RUN dotnet build "Bridgette.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "Bridgette.Api.csproj" -c Release -o /app/publish

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy the CA certificate into the container
COPY Bridgette.Api/ca.pem .
CMD ["dotnet", "Bridgette.Api.dll"]