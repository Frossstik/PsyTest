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
using PsyTest.ServiceIdentity.Features.UpdateProfile;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("identitydb")));

// Identity (но куки не делаем дефолтными)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// чтобы не было редиректа на /Account/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// gRPC
builder.Services.AddGrpc();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// ⚡️ делаем JWT дефолтной схемой
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false, // audience не проверяем
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Логирование событий
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("❌ JWT Error: " + ctx.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            Console.WriteLine("✅ Token validated!");
            return Task.CompletedTask;
        },
        OnChallenge = ctx =>
        {
            Console.WriteLine("⚠️ Challenge: " + ctx.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddScoped<JwtTokenGenerator>();
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service API", Version = "v1" });

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// применяем миграции
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var strategy = db.Database.CreateExecutionStrategy();
    strategy.Execute(() =>
    {
        var pendingMigrations = db.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            Console.WriteLine("Add migrations...");
            db.Database.Migrate();
            Console.WriteLine("Migrations complete.");
        }
        else
        {
            Console.WriteLine("New migrations are clear.");
        }
    });
}

app.MapGrpcService<IdentityGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// логирование запросов
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
        Console.WriteLine("Authorization header: " + context.Request.Headers["Authorization"]);
    else
        Console.WriteLine("Authorization header MISSING");

    Console.WriteLine("Request: " + context.Request.Method + " " + context.Request.Path);
    await next();
    Console.WriteLine("Response: " + context.Response.StatusCode);
});

app.UseAuthentication();
app.UseAuthorization();

// endpoints
app.MapGet("/", () => "Identity service is working!");

app.MapPost("/register", async (RegisterDto dto, IMediator mediator) =>
{
    var result = await mediator.Send(new RegisterUserCommand(
        dto.UserName,
        dto.Email,
        dto.Password,
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber));

    return result.Succeeded ? Results.Ok("User registered") : Results.BadRequest(result.Errors);
});

app.MapPost("/login", async (LoginDto dto, IMediator mediator) =>
{
    var token = await mediator.Send(new LoginUserCommand(dto.Email, dto.Password));
    return token is null ? Results.Unauthorized() : Results.Ok(new { token });
});

app.MapPut("/profile", async (UpdateProfileDto dto, IMediator mediator, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub");

    if (userId == null)
        return Results.Unauthorized();

    var result = await mediator.Send(new UpdateProfileCommand(
        userId,
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber,
        dto.Email,
        dto.Password
    ));

    return result.Succeeded ? Results.Ok("Profile updated") : Results.BadRequest(result.Errors);
}).RequireAuthorization();

app.MapGet("/profile", async (UserManager<ApplicationUser> userManager, ClaimsPrincipal user) =>
{
    Console.WriteLine("Claims on /profile:");
    foreach (var c in user.Claims)
        Console.WriteLine($"   {c.Type} = {c.Value}");

    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub");

    if (userId == null)
        return Results.Unauthorized();

    var appUser = await userManager.FindByIdAsync(userId);
    if (appUser == null)
        return Results.NotFound("User not found");

    return Results.Ok(new ProfileDto
    {
        Id = appUser.Id,
        UserName = appUser.UserName ?? string.Empty,
        Email = appUser.Email ?? string.Empty,
        FirstName = appUser.FirstName,
        LastName = appUser.LastName,
        PhoneNumber = appUser.PhoneNumber
    });
}).RequireAuthorization();

app.MapDefaultEndpoints();
app.UseCors();
app.Run();
