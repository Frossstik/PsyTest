using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PsyTest.ServiceIdentity.Common;
using PsyTest.ServiceIdentity.Application.LoginUser;
using PsyTest.ServiceIdentity.Application.RegisterUser;
using PsyTest.ServiceIdentity.Application.UpdateProfile;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using PsyTest.ServiceIdentity.Infrastructure;
using PsyTest.ServiceIdentity.Infrastructure.Grpc;
using PsyTest.ServiceIdentity.Domain.DTOs;
using PsyTest.ServiceIdentity.Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using PsyTest.ServiceIdentity.Application.ConfirmPasswordChange;
using PsyTest.ServiceIdentity.Application.SendChangePasswordEmail;

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
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

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

app.MapPost("/auth/register", async (RegisterDto dto, IMediator mediator) =>
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

app.MapPost("/auth/login", async (LoginDto dto, IMediator mediator) =>
{
    var token = await mediator.Send(new LoginUserCommand(dto.Email, dto.Password));
    return token is null ? Results.Unauthorized() : Results.Ok(new { token });
});

app.MapPut("/auth/profile", async (UpdateProfileDto dto, IMediator mediator, ClaimsPrincipal user) =>
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
        dto.Email
    ));

    return result.Succeeded ? Results.Ok("Profile updated") : Results.BadRequest(result.Errors);
}).RequireAuthorization();

app.MapGet("/auth/profile", async (UserManager<ApplicationUser> userManager, ClaimsPrincipal user) =>
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

// 1) юзер отправляет текущий пароль + новый пароль
//    мы генерим токен, шлём письмо со ссылкой на ФРОНТ
app.MapPost("/auth/change-password", async (
    ChangePasswordDto dto,
    ClaimsPrincipal user,
    IMediator mediator
) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub");

    if (userId == null)
        return Results.Unauthorized();

    // dto.ConfirmUrl теперь должен быть URL фронта, например:
    // https://my-frontend.ngrok-free.app/confirm-password-change
    var cmd = new SendChangePasswordEmailCommand(
        UserId: userId,
        CurrentPassword: dto.CurrentPassword,
        NewPassword: dto.NewPassword,
        ConfirmUrl: dto.ConfirmUrl // <--- фронтовый адрес без query
    );

    var result = await mediator.Send(cmd);

    return result.Succeeded
        ? Results.Ok(new { message = result.Message })
        : Results.BadRequest(result.Errors);
}).RequireAuthorization();


// 2) фронт (страница confirm-password-change) дергает это, чтобы реально применить новый пароль
app.MapPost("/auth/confirm-password-change", async (
    ConfirmPasswordDto dto,
    IMediator mediator
) =>
{
    // dto = { email, token, newPassword } из фронта
    if (string.IsNullOrWhiteSpace(dto.Email) ||
        string.IsNullOrWhiteSpace(dto.Token) ||
        string.IsNullOrWhiteSpace(dto.NewPassword))
    {
        return Results.BadRequest("Отсутствуют обязательные параметры.");
    }

    var result = await mediator.Send(
        new ConfirmPasswordChangeCommand(dto.Email, dto.Token, dto.NewPassword)
    );

    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    return Results.Ok(new { message = result.Message ?? "Пароль обновлён." });
});

//// подтверждение смены пароля (по ссылке из письма)
//app.MapGet("/auth/confirm-password-change", async (
//    string? email,
//    string? token,
//    string? newPassword,
//    IMediator mediator
//) =>
//{
//    if (string.IsNullOrWhiteSpace(email) ||
//        string.IsNullOrWhiteSpace(token) ||
//        string.IsNullOrWhiteSpace(newPassword))
//    {
//        return Results.Content(
//            "<h2>Ошибка</h2><p>Отсутствуют обязательные параметры.</p>",
//            "text/html; charset=utf-8",
//            statusCode: StatusCodes.Status400BadRequest
//        );
//    }

//    var result = await mediator.Send(
//        new ConfirmPasswordChangeCommand(email, token, newPassword)
//    );

//    if (!result.Succeeded)
//    {
//        var errors = string.Join("<br/>", result.Errors ?? Array.Empty<string>());
//        return Results.Content(
//            $"<h2>Не удалось изменить пароль</h2><p>{errors}</p>",
//            "text/html; charset=utf-8",
//            statusCode: StatusCodes.Status400BadRequest
//        );
//    }

//    return Results.Content(
//        "<h2>Готово ✅</h2>" +
//        "<p>Пароль успешно изменён. Теперь вы можете закрыть это окно и войти с новым паролем.</p>",
//        "text/html; charset=utf-8",
//        statusCode: StatusCodes.Status200OK
//    );
//});

app.MapDefaultEndpoints();
app.UseCors();
app.Run();
