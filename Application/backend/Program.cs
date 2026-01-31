
using StackExchange.Redis;
using Redis.OM;

using backend.Application.Interfaces;
using backend.Application.Services;

using backend.Infrastructure.Persistence.Redis;
using backend.Infrastructure.Persistence.Neo4j;
using backend.Infrastructure.Messaging.RabbitMQ;

using backend.Api.Hubs;
using backend.Api.Middleware;

using Neo4j.Driver;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "backend", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Unesi JWT token u formatu: Bearer {token}"
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


// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var cs = builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(cs!);
});

builder.Services.AddScoped<IRedisLockService, RedisLockService>();

// Redis OM 
builder.Services.AddSingleton<RedisConnectionProvider>();
builder.Services.AddScoped<IMessageRepository, RedisMessageRepository>();
builder.Services.AddScoped<ISessionRepository, RedisSessionRepository>();


// Neo4j
builder.Services.AddSingleton<IDriver>(_ =>
    GraphDatabase.Driver(
        "bolt://localhost:7687",
        AuthTokens.Basic("neo4j", "password")
    )
);

builder.Services.AddScoped<IJobRepository, Neo4jJobRepository>();
builder.Services.AddScoped<IUserRepository, Neo4jUserRepository>();
builder.Services.AddScoped<IConversationRepository, Neo4jConversationRepository>();



// Services
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IUserService, UserService>();


// RabbitMQ
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4209")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// SignalIR
builder.Services.AddSignalR();

//jwt
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});



var app = builder.Build();

var rabbitConsumer = new RabbitMqConsumer();
rabbitConsumer.Start();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DocumentHub>("/hubs/document");

app.Run();
