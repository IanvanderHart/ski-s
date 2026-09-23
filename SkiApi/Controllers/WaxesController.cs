using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiApi.Data;
using SkiApi.Models;

namespace SkiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaxesController : ControllerBase
{
    private readonly AppDbContext _context;

    public WaxesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Waxes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var waxes = await _context.Waxes.ToListAsync();
        return Ok(waxes);
    }

    // GET: api/Waxes/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var wax = await _context.Waxes.FindAsync(id);
        if (wax == null) return NotFound();
        return Ok(wax);
    }

    // POST: api/Waxes
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Wax wax)
    {
        _context.Waxes.Add(wax);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = wax.Id }, wax);
    }

    // PUT: api/Waxes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Wax wax)
    {
        var existing = await _context.Waxes.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = wax.Name;
        existing.Brand = wax.Brand;
        existing.Category = wax.Category;
        existing.Type = wax.Type;
        existing.TempMin = wax.TempMin;
        existing.TempMax = wax.TempMax;
        existing.HumidityMin = wax.HumidityMin;
        existing.HumidityMax = wax.HumidityMax;
        existing.SnowType = wax.SnowType;
        existing.TrackType = wax.TrackType;
        existing.Notes = wax.Notes;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // DELETE: api/Waxes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var wax = await _context.Waxes.FindAsync(id);
        if (wax == null) return NotFound();

        _context.Waxes.Remove(wax);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

