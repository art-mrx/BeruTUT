FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Store.slnx", "./"]
COPY ["src/Store.Domain/Store.Domain.csproj", "src/Store.Domain/"]
COPY ["src/Store.Application/Store.Application.csproj", "src/Store.Application/"]
COPY ["src/Store.Infrastructure/Store.Infrastructure.csproj", "src/Store.Infrastructure/"]
COPY ["src/Store.Api/Store.Api.csproj", "src/Store.Api/"]
RUN dotnet restore "src/Store.Api/Store.Api.csproj"

COPY . .
RUN dotnet publish "src/Store.Api/Store.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Store.Api.dll"]
