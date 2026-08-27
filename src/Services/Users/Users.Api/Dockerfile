FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Users/Users.Api/Users.Api.csproj", "Users.Api/"]
COPY ["src/Services/Users/Users.Application/Users.Application.csproj", "Users.Application/"]
COPY ["src/Services/Users/Users.Domain/Users.Domain.csproj", "Users.Domain/"]
COPY ["src/Services/Users/Users.Infrastructure/Users.Infrastructure.csproj", "Users.Infrastructure/"]
RUN dotnet restore "Users.Api/Users.Api.csproj"

COPY ["src/Services/Users/", "."]
WORKDIR /src/Users.Api
RUN dotnet publish -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:10.2026.08.26.20 AS final
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["Users.Api"]