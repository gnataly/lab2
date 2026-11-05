FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY CarRental.sln .
COPY src/RentalService/RentalService.csproj src/RentalService/
RUN dotnet restore src/RentalService/RentalService.csproj
COPY src/RentalService/ src/RentalService/
WORKDIR /src/src/RentalService
RUN dotnet publish RentalService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8060
EXPOSE 8060
ENTRYPOINT ["dotnet", "RentalService.dll"]