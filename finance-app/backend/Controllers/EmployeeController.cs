using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly FinanceAppDbContext _context;
        public EmployeeController(FinanceAppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var employees = await _context.Employees.ToListAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Bir hata oluştu.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var emp = await _context.Employees.FindAsync(id);
                if (emp == null)
                    return NotFound(new { error = "Employee bulunamadı.", employeeNumber = id });

                return Ok(emp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Bir hata oluştu.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee emp)
        {
            try
            {
                if (emp == null)
                    return BadRequest(new { error = "Girilen veri geçersiz." });

                if (await _context.Employees.AnyAsync(e => e.EmployeeNumber == emp.EmployeeNumber))
                    return Conflict(new { error = "Bu EmployeeNumber ile zaten bir çalışan mevcut.", employeeNumber = emp.EmployeeNumber });

                _context.Employees.Add(emp);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = emp.EmployeeNumber }, emp);
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "Veritabanı kaydı sırasında hata oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Bir hata oluştu.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employee emp)
        {
            if (id != emp.EmployeeNumber)
                return BadRequest(new { error = "ID ile employeeNumber eşleşmiyor." });

            try
            {
                var exists = await _context.Employees.AnyAsync(e => e.EmployeeNumber == id);
                if (!exists)
                    return NotFound(new { error = "Çalışan bulunamadı.", employeeNumber = id });

                _context.Entry(emp).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Çalışan güncellendi.", employee = emp });
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "Güncelleme esnasında hata oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Bir hata oluştu.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var emp = await _context.Employees.FindAsync(id);
                if (emp == null)
                    return NotFound(new { error = "Çalışan bulunamadı.", employeeNumber = id });

                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Çalışan silindi.", employee = emp });
            }
            catch (DbUpdateException dbEx)
            {
                return BadRequest(new { error = "Silme işlemi sırasında hata oluştu.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Bir hata oluştu.", details = ex.Message });
            }
        }
    }
}