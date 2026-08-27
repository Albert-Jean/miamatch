FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/ShoppingList/ShoppingList.Consumer/ShoppingList.Consumer.csproj", "ShoppingList.Consumer/"]
COPY ["src/Services/ShoppingList/ShoppingList.Application/ShoppingList.Application.csproj", "ShoppingList.Application/"]
COPY ["src/Services/ShoppingList/ShoppingList.Domain/ShoppingList.Domain.csproj", "ShoppingList.Domain/"]
COPY ["src/Services/ShoppingList/ShoppingList.Infrastructure/ShoppingList.Infrastructure.csproj", "ShoppingList.Infrastructure/"]
RUN dotnet restore "ShoppingList.Consumer/ShoppingList.Consumer.csproj"

COPY ["src/Services/ShoppingList/", "."]
WORKDIR /src/ShoppingList.Consumer
RUN dotnet publish -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:10.2026.08.26.20 AS final
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["ShoppingList.Consumer::ShoppingList.Consumer.Function::FunctionHandler"]