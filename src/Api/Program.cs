using Api.Endpoints;
using Api.Errors;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
string raw = builder.Configuration.GetConnectionString("AppDb")
    ?? "Data Source=data/app.db";
var connection = new SqliteConnectionStringBuilder(raw);
if (connection.DataSource != ":memory:")
{
    string path = Path.IsPathRooted(connection.DataSource)
        ? connection.DataSource
        : Path.Combine(builder.Environment.ContentRootPath, connection.DataSource);
    connection.DataSource = Path.GetFullPath(path);
    Directory.CreateDirectory(Path.GetDirectoryName(connection.DataSource)!);
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection.ConnectionString));
builder.Services.AddScoped<IOrderStore, EfOrderStore>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOrders();

app.Run();
