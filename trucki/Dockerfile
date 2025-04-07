FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["trucki.csproj", "."]
RUN dotnet restore "./trucki.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "trucki.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "trucki.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY otp_template.html /app/
COPY password_reset_template.html /app/
COPY welcome_email_template.html /app/
COPY invoice_email_template.html /app/
COPY order_template.html /app/
COPY trucki-c0df5-firebase-adminsdk-fbsvc-14d406ba99.json /app/
ENTRYPOINT ["dotnet", "trucki.dll"]
