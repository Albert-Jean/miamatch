using Microsoft.AspNetCore.Authentication.JwtBearer;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recipes.Application.Abstractions;
using Recipes.Application.Decks;
using Recipes.Infrastructure.Catalog;
using Recipes.Infrastructure.Configuration;
using Recipes.Infrastructure.Persistence;
using Recipes.Api.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// Must stay ahead of anything that reads configuration: it overrides appsettings and environment variables.
builder.Configuration.AddMiamMatchSecrets();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddDbContext<RecipesDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("RecipesDb")));
builder.Services.AddScoped<IDeckRepository, DeckRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<GetCurrentDeckHandler>();
builder.Services.AddScoped<GenerateDeckHandler>();
builder.Services.AddSingleton<IRecipeCatalog, SeedRecipeCatalog>();
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
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapRecipesEndpoints();
app.Run();