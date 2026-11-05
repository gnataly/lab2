FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY CarRental.sln .
COPY src/CarsService/CarsService.csproj src/CarsService/
RUN dotnet restore src/CarsService/CarsService.csproj
COPY src/CarsService/ src/CarsService/
WORKDIR /src/src/CarsService
RUN dotnet publish CarsService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8070
EXPOSE 8070
ENTRYPOINT ["dotnet", "CarsService.dll"]