using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiApi.Data;
using SkiApi.Models;

namespace SkiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoneGrindsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StoneGrindsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/StoneGrinds
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var grinds = await _context.StoneGrinds
            .OrderBy(g => g.Name)
            .ToListAsync();
        return Ok(grinds);
    }

    // GET: api/StoneGrinds/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var grind = await _context.StoneGrinds.FindAsync(id);
        if (grind == null) return NotFound();
        return Ok(grind);
    }

    // POST: api/StoneGrinds
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoneGrind grind)
    {
        if (string.IsNullOrWhiteSpace(grind.Name))
            return BadRequest("Name обязателен");

        _context.StoneGrinds.Add(grind);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = grind.Id }, grind);
    }

    // PUT: api/StoneGrinds/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StoneGrind updated)
    {
        var grind = await _context.StoneGrinds.FindAsync(id);
        if (grind == null) return NotFound();

        grind.Name = updated.Name;
        grind.TempMin = updated.TempMin;
        grind.TempMax = updated.TempMax;
        grind.SnowTypes = updated.SnowTypes;
        grind.TrackType = updated.TrackType;
        grind.Notes = updated.Notes;

        await _context.SaveChangesAsync();
        return Ok(grind);
    }

    // DELETE: api/StoneGrinds/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var grind = await _context.StoneGrinds.FindAsync(id);
        if (grind == null) return NotFound();

        // Проверяем, не используется ли в лыжах
        var usedBySki = await _context.SkiModels.AnyAsync(s => s.StoneGrindId == id);
        if (usedBySki)
            return Conflict(new { error = "Штайншлифт используется в лыжах. Сначала смените его у лыж." });

        _context.StoneGrinds.Remove(grind);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

