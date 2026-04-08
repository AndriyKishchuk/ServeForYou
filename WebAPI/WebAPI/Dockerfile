FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["WebAPI/WebAPI/WebAPI.csproj", "WebAPI/WebAPI/"]
COPY ["DataBase/DataBase/DataBase.csproj", "DataBase/DataBase/"]

RUN dotnet restore "WebAPI/WebAPI/WebAPI.csproj"

COPY . .
WORKDIR "/src/WebAPI/WebAPI"

RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WebAPI.dll"]
