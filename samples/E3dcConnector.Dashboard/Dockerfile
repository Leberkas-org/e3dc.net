FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ../../src/E3dcConnector/ src/E3dcConnector/
COPY ../../src/E3dcConnector.Typed/ src/E3dcConnector.Typed/
COPY samples/E3dcConnector.Dashboard/ samples/E3dcConnector.Dashboard/
RUN dotnet publish samples/E3dcConnector.Dashboard -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "E3dcConnector.Dashboard.dll"]
