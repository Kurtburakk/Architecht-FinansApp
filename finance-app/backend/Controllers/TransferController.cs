using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly FinanceAppDbContext _context;
        private readonly IExternalBankService _bankService;

        public TransferController(FinanceAppDbContext context, IExternalBankService bankService)
        {
            _context = context;
            _bankService = bankService;
        }

        // Tüm transferleri getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var transfers = await _context.Transfers.ToListAsync();
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Transferler getirilirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Tek bir transferi getir
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var t = await _context.Transfers.FindAsync(id);
                if (t == null)
                    return NotFound(new { error = "Transfer bulunamadı.", transferId = id });
                return Ok(t);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Transfer getirilirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Yeni transfer oluştur
        [HttpPost]
        public async Task<IActionResult> Create(Transfer t)
        {
            try
            {
                if (t == null)
                    return BadRequest(new { error = "Geçersiz transfer verisi gönderildi." });

                // Eğer dış banka transferi ise mock servisi tetikle
                if (!string.IsNullOrEmpty(t.ExternalBankIban))
                {
                    var result = await _bankService.SendTransferAsync(t.ExternalBankIban, t.Amount);
                    if (!result)
                        return BadRequest(new { error = "Dış bankaya transfer başarısız oldu (Mock API)!" });
                }
                _context.Transfers.Add(t);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "Transfer eklenirken veritabanı hatası oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Transfer eklenirken bir hata oluştu.", details = ex.Message });
            }
        }

        // Transferi güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transfer t)
        {
            if (id != t.Id)
                return BadRequest(new { error = "Parametre ile gönderilen ID ile transfer ID eşleşmiyor." });

            try
            {
                var exists = await _context.Transfers.AnyAsync(x => x.Id == id);
                if (!exists)
                    return NotFound(new { error = "Güncellenecek transfer bulunamadı.", transferId = id });

                _context.Entry(t).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Transfer güncellendi", transfer = t });
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

        // Transferi sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var t = await _context.Transfers.FindAsync(id);
                if (t == null)
                    return NotFound(new { error = "Silinecek transfer bulunamadı.", transferId = id });

                _context.Transfers.Remove(t);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Transfer silindi", transfer = t });
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