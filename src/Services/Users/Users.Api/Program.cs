using System.Text;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Users.Api;
using Users.Api.Endpoints;
using Users.Application.Abstractions;
using Users.Application.Households;
using Users.Application.Users;
using Users.Infrastructure;
using Users.Infrastructure.Configuration;
using Users.Infrastructure.Persistence.Repositories;
using Users.Infrastructure.Security;

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

builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("UsersDb")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<RegisterUserHandler, RegisterUserHandler>();
builder.Services.AddScoped<LoginHandler, LoginHandler>();
builder.Services.AddScoped<CreateHouseholdHandler, CreateHouseholdHandler>();
builder.Services.AddScoped<JoinHouseholdHandler, JoinHouseholdHandler>();

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapUserEndpoints();
app.MapHouseholdEndpoints();
app.Run();
