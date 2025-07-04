﻿using mts_integration.Application.AuthService;
using mts_integration.Application.DataService;
using mts_integration.Application.DataService.CachingServirce.Redis;
using mts_integration.RestAPI;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMemoryCache();


builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<IApiDataProvider, ApiDataProvider>();
builder.Services.AddSingleton<IGenerateData, GenerateData>();
builder.Services.AddSingleton<IDataCacheService, DataCacheService>();



builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));
builder.Services.AddSingleton<RedisBackupService>();
builder.Services.AddHostedService<RedisAutoBackupWorker>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();


app.UseCors("AllowAll");

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
