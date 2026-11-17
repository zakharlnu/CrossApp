using Api.Endpoints;
using Api.Errors;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
builder.Services.AddSingleton<IOrderStore, InMemoryOrderStore>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOrders();

app.Run();
