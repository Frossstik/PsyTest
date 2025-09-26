using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR;
using Psytest.ServiceMain.Infrastructure;
using Psytest.ServiceMain.Domain.Logic;
using Psytest.ServiceMain.Domain.Options;
using Polly;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
        options.Authority = builder.Configuration["IdentityService:Authority"];
        options.Audience = "main-api";
        options.RequireHttpsMetadata = false;
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
