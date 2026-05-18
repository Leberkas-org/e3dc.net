FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ../../src/E3dc.Net/ src/E3dc.Net/
COPY samples/E3dc.Net.Dashboard/ samples/E3dc.Net.Dashboard/
RUN dotnet publish samples/E3dc.Net.Dashboard -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "E3dc.Net.Dashboard.dll"]
