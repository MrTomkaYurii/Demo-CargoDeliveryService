FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["DriversManagement/src/DriversManagement/DriversManagement.csproj", "./DriversManagement/src/DriversManagement/"]
COPY ["SharedKernel/SharedKernel.csproj", "./SharedKernel/"]
RUN dotnet restore "./DriversManagement/src/DriversManagement/DriversManagement.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet build "DriversManagement/src/DriversManagement/DriversManagement.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "DriversManagement/src/DriversManagement/DriversManagement.csproj" -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "/app/DriversManagement.dll"]
