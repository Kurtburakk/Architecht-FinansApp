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
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Transfers.ToListAsync());

        // Tek bir transferi getir
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var t = await _context.Transfers.FindAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        // Yeni transfer oluştur
        [HttpPost]
        public async Task<IActionResult> Create(Transfer t)
        {
            // Eğer dış banka transferi ise mock servisi tetikle
            if (!string.IsNullOrEmpty(t.ExternalBankIban))
            {
                var result = await _bankService.SendTransferAsync(t.ExternalBankIban, t.Amount);
                if (!result)
                    return BadRequest("Dış bankaya transfer başarısız oldu (Mock API)!");
            }
            _context.Transfers.Add(t);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
        }

        // Transferi güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transfer t)
        {
            if (id != t.Id) return BadRequest();
            _context.Entry(t).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Transfer güncellendi", transfer = t });
        }

        // Transferi sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.Transfers.FindAsync(id);
            if (t == null) return NotFound();
            _context.Transfers.Remove(t);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Transfer silindi", transfer = t });
        }
    }
}