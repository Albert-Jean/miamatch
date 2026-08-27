FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Matching/Matching.Api/Matching.Api.csproj", "Matching.Api/"]
COPY ["src/Services/Matching/Matching.Application/Matching.Application.csproj", "Matching.Application/"]
COPY ["src/Services/Matching/Matching.Domain/Matching.Domain.csproj", "Matching.Domain/"]
COPY ["src/Services/Matching/Matching.Infrastructure/Matching.Infrastructure.csproj", "Matching.Infrastructure/"]
RUN dotnet restore "Matching.Api/Matching.Api.csproj"

COPY ["src/Services/Matching/", "."]
WORKDIR /src/Matching.Api
RUN dotnet publish -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:10.2026.08.26.20 AS final
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["Matching.Api"]