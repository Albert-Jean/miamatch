FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Recipes/Recipes.Api/Recipes.Api.csproj", "Recipes.Api/"]
COPY ["src/Services/Recipes/Recipes.Application/Recipes.Application.csproj", "Recipes.Application/"]
COPY ["src/Services/Recipes/Recipes.Domain/Recipes.Domain.csproj", "Recipes.Domain/"]
COPY ["src/Services/Recipes/Recipes.Infrastructure/Recipes.Infrastructure.csproj", "Recipes.Infrastructure/"]
RUN dotnet restore "Recipes.Api/Recipes.Api.csproj"

COPY ["src/Services/Recipes/", "."]
WORKDIR /src/Recipes.Api
RUN dotnet publish -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:10.2026.08.26.20 AS final
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["Recipes.Api"]