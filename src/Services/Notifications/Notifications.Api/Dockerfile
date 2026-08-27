FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Notifications/Notifications.Api/Notifications.Api.csproj", "Notifications.Api/"]
COPY ["src/Services/Notifications/Notifications.Application/Notifications.Application.csproj", "Notifications.Application/"]
COPY ["src/Services/Notifications/Notifications.Domain/Notifications.Domain.csproj", "Notifications.Domain/"]
COPY ["src/Services/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj", "Notifications.Infrastructure/"]
RUN dotnet restore "Notifications.Api/Notifications.Api.csproj"

COPY ["src/Services/Notifications/", "."]
WORKDIR /src/Notifications.Api
RUN dotnet publish -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:10.2026.08.26.20 AS final
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["Notifications.Api"]