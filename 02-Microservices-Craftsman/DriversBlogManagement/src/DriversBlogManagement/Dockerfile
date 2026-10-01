FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj", "./DriversBlogManagement/src/DriversBlogManagement/"]
COPY ["SharedKernel/SharedKernel.csproj", "./SharedKernel/"]
RUN dotnet restore "./DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet build "DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj" -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "/app/DriversBlogManagement.dll"]
