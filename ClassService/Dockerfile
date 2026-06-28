FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy csproj and restore NuGet packages
COPY *.csproj ./
RUN dotnet restore

# Copy remaining source code and publish in Release mode
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ClassService.dll"]
