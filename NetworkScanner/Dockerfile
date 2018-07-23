FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM bmccoy04/dotnet-core-2.1-runtime-with-nmap:latest As runtime
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "NetworkScanner.dll"]