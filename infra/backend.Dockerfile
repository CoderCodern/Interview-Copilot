# Multi-stage build for the .NET 10 API (Doc 08 §1, Doc 09 §3).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore with central package management (copy props + csproj first for layer caching).
COPY backend/Directory.Build.props backend/Directory.Packages.props backend/global.json ./backend/
COPY backend/src ./backend/src
RUN dotnet restore backend/src/InterviewCopilot.Api/InterviewCopilot.Api.csproj

RUN dotnet publish backend/src/InterviewCopilot.Api/InterviewCopilot.Api.csproj \
    -c Release -o /app --no-restore

# Minimal, non-root runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "InterviewCopilot.Api.dll"]
