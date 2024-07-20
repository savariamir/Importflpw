using ImportFlow;
using ImportFlow.Api;
using ImportFlow.Consumers;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Repositories;

var builder = WebApplication.CreateBuilder(args);

var configuration = ((IConfigurationBuilder)builder.Configuration).Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configuration);

builder.Services.AddTransient<IMessageConsumer<SupplierFilesDownloaded>, InitialLoadConsumer>();
builder.Services.AddTransient<IMessageConsumer<InitialLoadFinished>, TransformationConsumer>();
builder.Services.AddTransient<IMessageConsumer<TransformationFinished>, DataExportConsumer>();

builder.Services.AddSingleton<IImportFlowRepository, ImportFlowRepository>();

builder.Services.AddSingleton<IStateRepository<SupplierFilesDownloaded>, StateRepository<SupplierFilesDownloaded>>();
builder.Services.AddSingleton<IStateRepository<InitialLoadFinished>, StateRepository<InitialLoadFinished>>();
builder.Services.AddSingleton<IStateRepository<TransformationFinished>, StateRepository<TransformationFinished>>();
builder.Services.AddSingleton<IStateRepository<DataExported>, StateRepository<DataExported>>();

// builder.Services.AddSingleton<IEventRepository, EventRepository>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/push-api", async (PushApi pushApi) =>
    {
        await pushApi.StartAsync();
    })
    .WithName("PushApi")
    .WithOpenApi();

app.MapGet("/get", async (PushApi pushApi) =>
    {
       return await pushApi.GetAsync();
    })
    .WithName("Get")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}