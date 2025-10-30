FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Enable ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY 10xWarehouseNet.csproj ./
RUN dotnet restore 10xWarehouseNet.csproj
COPY . .
RUN dotnet publish 10xWarehouseNet.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Default environment variables (override in compose)
ENV ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=postgres;Username=postgres;Password=postgres" \
    Supabase__Url="" \
    Supabase__ServiceRoleKey=""

ENTRYPOINT ["dotnet", "10xWarehouseNet.dll"]


