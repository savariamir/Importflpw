using ImportFlow;
using ImportFlow.Api;
using ImportFlow.Consumers;
using ImportFlow.Domain.Repositories;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using ImportFlow.Repositories;
using ImportFlow.Repositories.V2;

var builder = WebApplication.CreateBuilder(args);

var configuration = ((IConfigurationBuilder)builder.Configuration).Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<PushApiV2>();
builder.Services.AddMassTransit(configuration);

builder.Services.AddSingleton<IImportFlowRepositoryV2, ImportFlowRepositoryV2>();


builder.Services.AddSingleton<IStateRepositoryV2<SupplierFilesDownloaded>, StateRepositoryV2<SupplierFilesDownloaded>>();
builder.Services.AddSingleton<IStateRepositoryV2<InitialLoadFinished>, StateRepositoryV2<InitialLoadFinished>>();
builder.Services.AddSingleton<IStateRepositoryV2<TransformationFinished>, StateRepositoryV2<TransformationFinished>>();
builder.Services.AddSingleton<IStateRepositoryV2<DataExported>, StateRepositoryV2<DataExported>>();

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

app.MapGet("/push-api", async (PushApiV2 pushApi) => { await pushApi.StartAsync(); })
    .WithName("PushApi")
    .WithOpenApi();

app.MapGet("/get", async (PushApiV2 pushApi) => await pushApi.GetAsync())
    .WithName("Get")
    .WithOpenApi();

app.MapGet("/get-list", async (PushApiV2 pushApi) => await pushApi.GetImportFlowListAsync())
    .WithName("GetList")
    .WithOpenApi();


app.MapGet("/get-list/{id}", async (PushApiV2 pushApi, Guid id) =>
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