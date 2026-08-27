using Amazon.SQS;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ShoppingList.Api.Endpoints;
using ShoppingList.Application.Abstractions;
using ShoppingList.Application.ShoppingListItems;
using ShoppingList.Infrastructure;
using ShoppingList.Infrastructure.Persistence;
using ShoppingList.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ShoppingListDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ShoppingListDb")));
builder.Services.AddScoped<IShoppingListItemRepository, ShoppingListItemRepository>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateLifetime = true
        };
    });
builder.Services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(
    new Amazon.Runtime.BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"]),
    new AmazonSQSConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]) }));
builder.Services.AddScoped<AddMatchedRecipeIngredientsHandler>();
builder.Services.AddHttpClient<IRecipeClient, RecipeClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:RecipesApiBaseUrl"]!);
});
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapShoppingListEndpoints();
app.Run();
