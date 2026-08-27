using Amazon.Lambda.AspNetCoreServer.Hosting;
using Amazon.SimpleNotificationService;
using Matching.Api.Endpoints;
using Matching.Application.Abstractions;
using Matching.Application.Swipes;
using Matching.Infrastructure.Clients;
using Matching.Infrastructure.Configuration;
using Matching.Infrastructure.Persistence;
using Matching.Infrastructure.Persistence.Messaging;
using Matching.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddDbContext<MatchingDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MatchingDb")));
builder.Services.AddScoped<ISwipeRepository, SwipeRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<RecordSwipeHandler>();
builder.Services.AddHttpClient<IRecipeClient, RecipeClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:RecipesApiBaseUrl"]!);
});
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
// No explicit credentials: the SDK resolves them from the Lambda execution role in AWS
// and from the local AWS profile in development. The role needs sns:Publish on the topic.
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(
    new AmazonSimpleNotificationServiceConfig {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]) }));
builder.Services.AddScoped<IMatchEventPublisher, MatchEventPublisher>();
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
app.MapMatchingEndpoints();
app.Run();

