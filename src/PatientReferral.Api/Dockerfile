# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PatientReferral.sln ./
COPY src/PatientReferral.Api/PatientReferral.Api.csproj src/PatientReferral.Api/
COPY src/PatientReferral.Application/PatientReferral.Application.csproj src/PatientReferral.Application/
COPY src/PatientReferral.Infrastructure/PatientReferral.Infrastructure.csproj src/PatientReferral.Infrastructure/
RUN dotnet restore src/PatientReferral.Api/PatientReferral.Api.csproj

COPY . .
RUN dotnet publish src/PatientReferral.Api/PatientReferral.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "PatientReferral.Api.dll"]
