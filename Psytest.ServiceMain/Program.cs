using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR;
using Psytest.ServiceMain.Infrastructure;
using Psytest.ServiceMain.Domain.Logic;
using Psytest.ServiceMain.Domain.Options;
using Polly;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static PsyTest.identity.Identity;
using Psytest.ServiceMain;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Aspire defaults
builder.AddServiceDefaults();

// DbContext
builder.Services.AddDbContext<MainDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("maindb"));
    //options.ConfigureWarnings(w =>
    //    w.Ignore(RelationalEventId.PendingModelChangesWarning));
});



// Настройки для Reports
builder.Services.Configure<ReportsOptions>(builder.Configuration.GetSection("Reports"));

// CQRS (MediatR)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));



// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };


        // Логи
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Ошибка аутентификации: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var claims = string.Join(", ", context.Principal.Claims.Select(c => $"{c.Type}:{c.Value}"));
                Console.WriteLine($"Токен валидирован. Claims: {claims}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("Вызван Challenge — токен не прошёл проверку или отсутствует");
                return Task.CompletedTask;
            }
        };
    });

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Main Service API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT токен",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

builder.Services.AddGrpcClient<IdentityClient>(o =>
{
    o.Address = new Uri("https://localhost:7182"); // имя контейнера из Aspire
});

builder.Services.AddScoped<IdentityGrpcClient>();

// Domain Logic
builder.Services.AddScoped<ITestProcessor, LusherTestProcessor>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<MainDbContext>();

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

app.MapGet("/", () => "Main service is working!");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();
