using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StackExchange.Redis;

using Push.Messaging.Api.Middlewares;
using Push.Messaging.Api.Authorization;
using Push.Messaging.Api.Workers;

using Push.Messaging.Application.Interfaces;
using Push.Messaging.Application.Services;

using Push.Messaging.Data;
using Push.Messaging.Data.Entities;
using Push.Messaging.Data.Interfaces;
using Push.Messaging.Data.Repositories;

using Push.Messaging.Infrastructure.Cache;
using Push.Messaging.Infrastructure.RateLimit;
using Push.Messaging.Infrastructure.Options;

using Push.Messaging.Shared.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

#region Controllers & API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
#endregion

#region Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));
#endregion

#region Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis")!));
#endregion

#region Data Layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
#endregion

#region Infrastructure
builder.Services.AddSingleton<IUserCache, RedisUserCache>();
builder.Services.AddSingleton<RedisRateLimiter>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
#endregion

#region Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITokenService, TokenService>();  
#endregion

#region RabbitMQ
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<RabbitMQ.Client.IConnectionFactory>(sp =>
{
    var options = sp
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>()
        .Value;

    return new RabbitMQ.Client.ConnectionFactory
    {
        HostName = options.Host,
    };
});
#endregion

#region Authorization

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ActiveUser", policy =>
        policy.Requirements.Add(new ActiveUserRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, ActiveUserHandler>();
#endregion

#region Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region Background Workers
builder.Services.AddHostedService<UserCacheRefreshWorker>();
#endregion

var app = builder.Build();

#region Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            UserName = "testuser",
            IsActive = true
        });

        db.SaveChanges();
    }
}
#endregion

#region Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = string.Empty;
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Push Messaging API v1");
    });
}

app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

await app.RunAsync();