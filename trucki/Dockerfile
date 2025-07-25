FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["trucki.csproj", "trucki/"]
RUN dotnet restore "trucki/trucki.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/trucki"
RUN dotnet build "trucki.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "trucki.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy necessary files
COPY otp_template.html ./
COPY password_reset_template.html ./
COPY welcome_email_template.html ./
COPY invoice_email_template.html ./
COPY order_template.html ./
COPY trucki-c0df5-firebase-adminsdk-fbsvc-14d406ba99.json ./

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "trucki.dll"]