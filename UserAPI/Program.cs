using UserAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ DB Context
builder.Services.AddDbContext<MyDbDatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TestAPIConnection")));

// ✅ CORS add kar rahe hain
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")   
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS yaha use karna hai (Authorization se pehle)
app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();

app.Run();
