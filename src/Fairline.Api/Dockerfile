FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/Fairline.Api/Fairline.Api.csproj src/Fairline.Api/
COPY src/Fairline.Application/Fairline.Application.csproj src/Fairline.Application/
COPY src/Fairline.Abstractions/Fairline.Abstractions.csproj src/Fairline.Abstractions/
COPY src/Fairline.Domain/Fairline.Domain.csproj src/Fairline.Domain/
COPY src/Fairline.Infrastructure/Fairline.Infrastructure.csproj src/Fairline.Infrastructure/
COPY src/Fairline.ServiceDefaults/Fairline.ServiceDefaults.csproj src/Fairline.ServiceDefaults/
RUN dotnet restore src/Fairline.Api/Fairline.Api.csproj

COPY src/ src/
RUN dotnet publish src/Fairline.Api/Fairline.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Fairline.Api.dll"]
