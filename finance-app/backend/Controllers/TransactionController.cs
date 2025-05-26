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

        // TÜM işlemler (çalışan bağımsız)
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _context.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Tüm işlemler getirilirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Bir çalışanın TÜM işlemleri
        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int employeeNumber)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Where(t => t.EmployeeNumber == employeeNumber)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "İşlemler getirilirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Kategoriye göre işlemler (employee bazında)
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategory([FromQuery] int employeeNumber, [FromQuery] string category)
        {
            try
            {
                var list = await _context.Transactions
                    .Where(t => t.EmployeeNumber == employeeNumber && t.Category == category)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Kategoriye göre işlemler getirilirken hata.", details = ex.Message });
            }
        }

        // Tarih aralığına göre işlemler (employee bazında)
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDateRange([FromQuery] int employeeNumber, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var list = await _context.Transactions
                    .Where(t => t.EmployeeNumber == employeeNumber && t.TransactionDate >= start && t.TransactionDate <= end)
                    .OrderBy(t => t.TransactionDate)
                    .ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Tarihe göre işlemler getirilirken hata.", details = ex.Message });
            }
        }

        // Karma filtre (employee bazında: kategori ve/veya tarih aralığı)
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] int employeeNumber, [FromQuery] string? category, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            try
            {
                var query = _context.Transactions
                    .Where(t => t.EmployeeNumber == employeeNumber);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(t => t.Category == category);
                if (start.HasValue)
                    query = query.Where(t => t.TransactionDate >= start.Value);
                if (end.HasValue)
                    query = query.Where(t => t.TransactionDate <= end.Value);

                var result = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Filtrelenmiş işlemler getirilirken hata.", details = ex.Message });
            }
        }

        // Kategorilere göre toplam tutar ve işlem adedi (employee bazında)
        [HttpGet("summary/by-category")]
        public async Task<IActionResult> GetSummaryByCategory([FromQuery] int employeeNumber)
        {
            try
            {
                var summary = await _context.Transactions
                    .Where(t => t.EmployeeNumber == employeeNumber)
                    .GroupBy(t => t.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(x => x.Amount),
                        Count = g.Count()
                    })
                    .ToListAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Kategoriye göre özet alınırken hata.", details = ex.Message });
            }
        }

        // Tek işlem getir
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var t = await _context.Transactions.FindAsync(id);
                if (t == null)
                    return NotFound(new { error = "İşlem bulunamadı.", transactionId = id });
                return Ok(t);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "İşlem getirilirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Yeni işlem ekle
        [HttpPost]
        public async Task<IActionResult> Create(Transaction t)
        {
            try
            {
                if (t == null)
                    return BadRequest(new { error = "Geçersiz işlem verisi gönderildi." });

                _context.Transactions.Add(t);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "İşlem eklenirken veritabanı hatası oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "İşlem eklenirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Belirli bir tutarı döviz olarak hesapla (örnek üçüncü parti servis entegrasyonu)
        [HttpGet("convert/{id}/{targetCurrency}")]
        public async Task<IActionResult> ConvertTransactionAmount(int id, string targetCurrency)
        {
            try
            {
                var t = await _context.Transactions.FindAsync(id);
                if (t == null)
                    return NotFound(new { error = "İşlem bulunamadı.", transactionId = id });

                var converted = await _currencyService.ConvertAsync(t.Amount, "TRY", targetCurrency.ToUpper());
                return Ok(new
                {
                    originalCurrency = "TRY",
                    originalAmount = t.Amount,
                    targetCurrency = targetCurrency.ToUpper(),
                    convertedAmount = converted
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Döviz dönüşümü sırasında bir hata oluştu.", details = ex.Message });
            }
        }

        // İşlemi güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transaction t)
        {
            if (id != t.Id)
                return BadRequest(new { error = "Parametre ile gönderilen ID ile transaction ID eşleşmiyor." });

            try
            {
                var exists = await _context.Transactions.AnyAsync(x => x.Id == id);
                if (!exists)
                    return NotFound(new { error = "Güncellenecek işlem bulunamadı.", transactionId = id });

                _context.Entry(t).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Masraf güncellendi", transaction = t });
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "Güncelleme sırasında veritabanı hatası oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Güncelleme sırasında bir hata oluştu.", details = ex.Message });
            }
        }

        // İşlemi sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var t = await _context.Transactions.FindAsync(id);
                if (t == null)
                    return NotFound(new { error = "Silinecek işlem bulunamadı.", transactionId = id });

                _context.Transactions.Remove(t);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Masraf silindi", transaction = t });
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "Silme sırasında veritabanı hatası oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Silme sırasında bir hata oluştu.", details = ex.Message });
            }
        }
    }
}