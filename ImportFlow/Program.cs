using ImportFlow;
using ImportFlow.Api;
using ImportFlow.Application;
using ImportFlow.Events;
using ImportFlow.Framework;
using ImportFlow.Infrastructure;
using ImportFlow.Infrastructure.Consumers;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var configuration = ((IConfigurationBuilder)builder.Configuration).Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<PushApi>();
builder.Services.AddMessaging(configuration);
builder.Services.AddImportFlowServices();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000") // Replace with your allowed origin
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// builder.Services.AddSingleton<IEventRepository, EventRepository>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/push-api", async (PushApi pushApi) => { await pushApi.StartAsync(); })
    .WithName("PushApi")
    .WithOpenApi();

app.MapGet("/get", async (ImportMonitoring importFlowService) => await importFlowService.GetAllAsync())
    .WithName("Get")
    .WithOpenApi();

app.MapGet("/get-list", async (ImportMonitoring importFlowService) => await importFlowService.GetImportFlowListAsync())
    .WithName("GetList")
    .WithOpenApi();


app.MapGet("/get-states", async (ImportMonitoring importFlowService, Guid id) => await importFlowService.GetStates(id))
    .WithName("GetStates")
    .WithOpenApi();

app.MapPost("/send",
        async (MessageRePublisher sender, [FromBody] MessageCommand command) => await sender.ResendAsync(command))
    .WithName("Send")
    .WithOpenApi();


app.MapGet("/get/{id}", async (ImportMonitoring importFlowService, Guid id) => await importFlowService.GatByIdAsync(id))
    .WithName("GetById")
    .WithOpenApi();


app.Run();