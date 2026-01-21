using StackExchange.Redis;
using backend.Application.Interfaces;
using backend.Infrastructure.Persistence.Redis;
using Neo4j.Driver;
using backend.Infrastructure.Persistence.Neo4j;
using backend.Application.Services;
using backend.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var cs = builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(cs!);
});

builder.Services.AddScoped<IRedisLockService, RedisLockService>();


// Neo4j
builder.Services.AddSingleton<IDriver>(_ =>
    GraphDatabase.Driver(
        "bolt://localhost:7687",
        AuthTokens.Basic("neo4j", "password")
    )
);

builder.Services.AddScoped<IJobRepository, Neo4jJobRepository>();


// Services
builder.Services.AddScoped<IJobService, JobService>();


// Dummy publisher (dok ne uvedemo RabbitMQ)
builder.Services.AddScoped<IMessagePublisher, DummyMessagePublisher>();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
