using Microsoft.EntityFrameworkCore;
using Payments.Application.Common.Interfaces;
using Payments.Application.Payments.CreatePayment;
using Payments.Infrastructure.Idempotency;
using Payments.Infrastructure.Persistence;
using StackExchange.Redis;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Redis connection (singleton is the recommended pattern for StackExchange.Redis)
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddScoped<IIdempotencyStore, RedisIdempotencyStore>();

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PaymentsDB")));

builder.Services.AddScoped<IPaymentsDbContext>(sp => sp.GetRequiredService<PaymentsDbContext>());
builder.Services.AddScoped<CreatePaymentHandler>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();


