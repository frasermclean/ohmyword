FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy solution and project files and restore dependencies
COPY Api/Api.csproj Api/
COPY Core/Core.csproj Core/
COPY Domain/Domain.csproj Domain/
COPY Integrations/Integrations.csproj Integrations/
RUN dotnet restore --use-lock-file Api/Api.csproj

# Copy source files and publish app
COPY . .
RUN dotnet publish --configuration Release --output /app Api/Api.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OhMyWord.Api.dll"]
