using MassTransit;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;
using Model = Stock.API.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using IServiceScope scope = app.Services.CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
var collection = mongoDbService.GetCollection<Model.Stock>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 200 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 100 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 50 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 10 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 30 });
}


app.Run();
