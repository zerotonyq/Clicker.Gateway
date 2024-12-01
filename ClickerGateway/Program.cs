using System.Text;
using ClickerGateway;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Добавляем ocelot.json для маршрутизации
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Zerotonyq",
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey("bobikBobinski228pukpuk0070000000!!!"u8.ToArray()),
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<ApiClassGenerator>();

var app = builder.Build();

var myClass = app.Services.GetRequiredService<ApiClassGenerator>();
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await myClass.GenerateApiClass(new List<string>()
    {
        "http://localhost:5000/swagger/docs//ClickerAuth",
        "http://localhost:5000/swagger/docs//Clicker"
    });
});

// Настройка Swagger UI для Ocelot
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs"; // Убедитесь, что путь совпадает с основным API
});

// Перенаправление HTTP на HTTPS
app.UseHttpsRedirection();

// Инициализация Ocelot middleware, которую нужно обязательно подождать
await app.UseOcelot();

// Запуск приложения
app.Run();

