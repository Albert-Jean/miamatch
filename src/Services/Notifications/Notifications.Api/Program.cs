using Amazon.SQS;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notifications.Api.Endpoints;
using Notifications.Application.Abstractions;
using Notifications.Application.Notifications;
using Notifications.Application.PushSubscriptions;
using Notifications.Infrastructure.Client;
using Notifications.Infrastructure.Clients;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddOpenApi();

const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationsDb")));

builder.Services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
builder.Services.AddScoped<RegisterPushSubscriptionHandler>();
builder.Services.AddScoped<SendMatchNotificationHandler>();

builder.Services.AddHttpClient<IHouseholdMembersClient, HouseholdMembersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UsersApiBaseUrl"]!);
});

builder.Services.AddSingleton<IPushNotificationSender, WebPushNotificationSender>();

builder.Services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(
    new Amazon.Runtime.BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"]),
    new AmazonSQSConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]) }));
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
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

var app = builder.Build();

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
app.MapNotificationsEndpoints();

app.Run();