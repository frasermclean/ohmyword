# This Dockerfile is used to build a runtime image for the published version of the API.
# It is assumed that the published version of the API is located in the publish/ directory.

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY publish/ ./
EXPOSE 80
ENTRYPOINT ["dotnet", "OhMyWord.Api.dll"]