using ImportFlow;
using ImportFlow.Api;
using ImportFlow.Consumers;
using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Repositories;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var configuration = ((IConfigurationBuilder)builder.Configuration).Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<PushApi>();
builder.Services.AddMassTransit(configuration);

builder.Services.AddSingleton<IImportFlowRepository, ImportFlowRepository>();

builder.Services.AddSingleton<IStateRepository<SupplierFilesDownloaded>, StateRepository<SupplierFilesDownloaded>>();
builder.Services.AddSingleton<IStateRepository<InitialLoadFinished>, StateRepository<InitialLoadFinished>>();
builder.Services.AddSingleton<IStateRepository<TransformationFinished>, StateRepository<TransformationFinished>>();
builder.Services.AddSingleton<IStateRepository<DataExported>, StateRepository<DataExported>>();

builder.Services.AddScoped<IMessageConsumer<SupplierFilesDownloaded>, InitialLoadConsumer>();
builder.Services.AddScoped<IMessageConsumer<InitialLoadFinished>, TransformationConsumer>();
builder.Services.AddScoped<IMessageConsumer<TransformationFinished>, DataExportConsumer>();


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

app.MapGet("/get", async (PushApi pushApi) => await pushApi.GetAsync())
    .WithName("Get")
    .WithOpenApi();

app.MapGet("/get-list", async (PushApi pushApi) => await pushApi.GetImportFlowListAsync())
    .WithName("GetList")
    .WithOpenApi();


app.MapGet("/get-list/{id}", async (PushApi pushApi, Guid id) =>
    {
        return await pushApi.GatByIdAsync(id);
    })
    .WithName("GetById")
    .WithOpenApi();


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}