using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PsyTest.ServiceIdentity;
using PsyTest.ServiceIdentity.Common;
using PsyTest.ServiceIdentity.DTOs;
using PsyTest.ServiceIdentity.Entities;
using PsyTest.ServiceIdentity.Features.LoginUser;
using PsyTest.ServiceIdentity.Features.RegisterUser;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("identitydb")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddGrpc();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services.AddAuthorization();


builder.Services.AddAuthentication("JwtBearer")
    .AddJwtBearer("JwtBearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<JwtTokenGenerator>();

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

var strategy = db.Database.CreateExecutionStrategy();

strategy.Execute(() =>
{
    var pendingMigrations = db.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
    {
        Console.WriteLine("Применяем миграции...");
        db.Database.Migrate();
        Console.WriteLine("Миграции применены.");
    }
    else
    {
        Console.WriteLine("Новых миграций нет.");
    }
});

app.MapGrpcService<IdentityGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Identity service is working!");

app.MapPost("/register", async (RegisterDto dto, IMediator mediator) =>
{
    var result = await mediator.Send(new RegisterUserCommand(dto.Email, dto.Password));
    return result.Succeeded ? Results.Ok("User registered") : Results.BadRequest(result.Errors);
});

app.MapPost("/login", async (LoginDto dto, IMediator mediator) =>
{
    var token = await mediator.Send(new LoginUserCommand(dto.Email, dto.Password));
    return token is null ? Results.Unauthorized() : Results.Ok(new { token });
});

app.Run();
