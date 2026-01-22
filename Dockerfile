# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Tlaoami.sln ./
COPY src/Tlaoami.API/Tlaoami.API.csproj src/Tlaoami.API/
COPY src/Tlaoami.Application/Tlaoami.Application.csproj src/Tlaoami.Application/
COPY src/Tlaoami.Domain/Tlaoami.Domain.csproj src/Tlaoami.Domain/
COPY src/Tlaoami.Infrastructure/Tlaoami.Infrastructure.csproj src/Tlaoami.Infrastructure/
RUN dotnet restore Tlaoami.sln

COPY src ./src
RUN dotnet publish src/Tlaoami.API/Tlaoami.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 3000
ENV ASPNETCORE_URLS="http://0.0.0.0:3000"
ENV PORT=3000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tlaoami.API.dll"]
