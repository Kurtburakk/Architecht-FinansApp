using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly FinanceAppDbContext _context;
        private readonly ICurrencyService _currencyService;

        public TransactionController(FinanceAppDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // Tüm işlemleri getir
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Transactions.ToListAsync());

        // Tek bir işlemi getir
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var t = await _context.Transactions.FindAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        // Yeni işlem ekle
        [HttpPost]
        public async Task<IActionResult> Create(Transaction t)
        {
            _context.Transactions.Add(t);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
        }

        // Belirli bir tutarı döviz olarak hesapla (örnek üçüncü parti servis entegrasyonu)
        [HttpGet("convert/{id}/{targetCurrency}")]
        public async Task<IActionResult> ConvertTransactionAmount(int id, string targetCurrency)
        {
            var t = await _context.Transactions.FindAsync(id);
            if (t == null) return NotFound();

            var converted = await _currencyService.ConvertAsync(t.Amount, "TRY", targetCurrency.ToUpper());
            return Ok(new
            {
                originalCurrency = "TRY",
                originalAmount = t.Amount,
                targetCurrency = targetCurrency.ToUpper(),
                convertedAmount = converted
            });
        }

        // İşlemi güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transaction t)
        {
            if (id != t.Id) return BadRequest();
            _context.Entry(t).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Masraf güncellendi", transaction = t });
        }

        // İşlemi sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.Transactions.FindAsync(id);
            if (t == null) return NotFound();
            _context.Transactions.Remove(t);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Masraf silindi", transaction = t });
        }
    }
}