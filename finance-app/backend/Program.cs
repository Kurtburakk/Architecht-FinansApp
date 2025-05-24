using Backend.Models;
using Microsoft.EntityFrameworkCore;

// ... diğer kodların üstünde
using CsvHelper;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FinanceAppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- SEED KODU EKLE ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceAppDbContext>();
    SeedData.SeedEmployeesFromCsv(db, Path.Combine(Directory.GetCurrentDirectory(), "data.csv"));
}
// --- SEED KODU SONU ---

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.Run();