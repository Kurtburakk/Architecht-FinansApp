using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ICurrencyService, MockCurrencyService>();
builder.Services.AddScoped<IExternalBankService, MockExternalBankService>();
builder.Services.AddHttpClient<ICurrencyService, FrankfurterCurrencyService>();

builder.Services.AddDbContext<FinanceAppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")))
);

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

app.UseDefaultFiles(); // ÖNCE BUNU
app.UseStaticFiles();  // SONRA BUNU

app.UseAuthorization();
app.MapControllers();

app.Run();