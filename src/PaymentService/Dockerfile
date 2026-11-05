FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY CarRental.sln .
COPY src/PaymentService/PaymentService.csproj src/PaymentService/
RUN dotnet restore src/PaymentService/PaymentService.csproj
COPY src/PaymentService/ src/PaymentService/
WORKDIR /src/src/PaymentService
RUN dotnet publish PaymentService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8050
EXPOSE 8050
ENTRYPOINT ["dotnet", "PaymentService.dll"]