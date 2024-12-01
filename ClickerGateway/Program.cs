using ClickerGateway;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Authorization;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
            ClockSkew = TimeSpan.Zero
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
        "http://localhost:5000/swagger/docs//Clicker",
        "http://localhost:5000/swagger/docs//AdminApi"
    });
});

// Настройка Swagger UI для Ocelot
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs"; // Убедитесь, что путь совпадает с основным API
});


app.UseHttpsRedirection();


await app.UseOcelot( new OcelotPipelineConfiguration(){AuthorizationMiddleware = async (ctx, next) =>
{
    if (!ctx.Items.DownstreamRoute().RouteClaimsRequirement.TryGetValue("Role", out var requiredRoleString))
    {
        await next.Invoke();
        return;
    }

    Console.WriteLine(ctx.Items.DownstreamRoute().RouteClaimsRequirement["Role"]);
    Console.WriteLine(ctx.User.Claims.First(c => c.Type == "Role").Value);
    
    var roles = ctx.Items.DownstreamRoute().RouteClaimsRequirement["Role"].Split(',');
    var userRoles = ctx.User.Claims.First(c => c.Type == "Role").Value.Split(',');


    if (userRoles.Any(role => roles.Contains(role)))
    {
        await next.Invoke();
    }
    else
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        await ctx.Response.WriteAsync("Access denied: insufficient roles.");
    }
    
}});

app.Run();