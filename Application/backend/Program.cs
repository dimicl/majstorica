using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using backend.Api.Hubs;
using backend.Api.Middleware;
using backend.Application.Interfaces;
using backend.Application.Services;
using backend.Infrastructure.Messaging.RabbitMQ;
using backend.Infrastructure.Persistence.MongoDb;
using backend.Infrastructure.Persistence.Neo4j;
using backend.Infrastructure.Persistence.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;
using Redis.OM;
using StackExchange.Redis;


// MongoDB: registruj Guid serializer da ne baca BsonSerializationException (Unspecified)
MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(
    new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
builder.Services.AddScoped<IRedisListCache, RedisMastersListCache>();

// Redis OM 
builder.Services.AddSingleton<RedisConnectionProvider>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISessionRepository, RedisSessionRepository>();


// Neo4j
builder.Services.AddSingleton<IDriver>(_ =>
{
    var neo4jUri = builder.Configuration["Neo4j:Uri"];
    var neo4jUser = builder.Configuration["Neo4j:User"];
    var neo4jPassword = builder.Configuration["Neo4j:Password"];

    if (string.IsNullOrWhiteSpace(neo4jUri) ||
        string.IsNullOrWhiteSpace(neo4jUser) ||
        string.IsNullOrWhiteSpace(neo4jPassword))
    {
        throw new InvalidOperationException(
            "Neo4j konfiguracija nije kompletna. Proveri Neo4j:Uri/User/Password u appsettings.json (ili env var)."
        );
    }

    return GraphDatabase.Driver(
        neo4jUri,
        AuthTokens.Basic(neo4jUser, neo4jPassword)
    );
});

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cs = builder.Configuration.GetConnectionString("DbConnection");
    return new MongoClient(cs);
});
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("Majstorica");
});
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<MongoJobRepository>();
builder.Services.AddScoped<IJobGraphRepository, Neo4jJobGraphRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();

// Neo4j (graph: minimal User/Job nodes, relationships)
builder.Services.AddScoped<IUserGraphSync, Neo4jUserGraphRepository>();
builder.Services.AddScoped<IGraphQueryRepository, Neo4jGraphQueryRepository>();

// Services
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobRequestRealtimeSender, backend.Api.Hubs.SignalRJobRequestSender>();
builder.Services.AddScoped<backend.Application.Interfaces.IJobRequestNotifier, backend.Application.Services.JobRequestNotifier>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConversationService, ConversationService>();


// RabbitMQ
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<backend.Infrastructure.Messaging.RabbitMQ.RabbitMqSignalRHostedService>();

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
    // SignalR: negotiate šalje token u Authorization headeru, WebSocket u query stringu (access_token)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            if (!path.StartsWithSegments("/hubs/document", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            var token = context.Request.Query["access_token"].FirstOrDefault()
                ?? context.Request.Query["token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token.Substring(7) : token;
                return Task.CompletedTask;
            }
            // WebSocket nema header; za negotiate klijent šalje Bearer u headeru – pročitaj ga
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                context.Token = authHeader.Substring(7);
            return Task.CompletedTask;
        }
    };
});



var app = builder.Build();

// Redis OM: kreiraj indekse da ne baca "no such index" pri upitima
try
{
    var redisProvider = app.Services.GetRequiredService<RedisConnectionProvider>();
    redisProvider.Connection.CreateIndex(typeof(backend.Infrastructure.Persistence.Redis.Entities.ChatMessageDocument));
    redisProvider.Connection.CreateIndex(typeof(backend.Infrastructure.Persistence.Redis.Entities.UserSessionDocument));
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Redis indeksi nisu kreirani (možda već postoje). Nastavljam.");
}

// RabbitMQ consumer za SignalR je registrovan kao HostedService (RabbitMqSignalRHostedService).

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
