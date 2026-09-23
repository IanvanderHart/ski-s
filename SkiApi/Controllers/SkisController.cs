using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiApi.Data;
using SkiApi.Models;

namespace SkiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkisController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkisController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Skis — все лыжи со связанными штайншлифтами
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var skis = await _context.SkiModels
            .Include(s => s.StoneGrind)
            .OrderBy(s => s.Style)
            .ThenBy(s => s.Brand)
            .ToListAsync();
        return Ok(skis);
    }

    // GET: api/Skis/5 — конкретная пара
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ski = await _context.SkiModels
            .Include(s => s.StoneGrind)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (ski == null) return NotFound();
        return Ok(ski);
    }

    // POST: api/Skis — создать пару
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SkiModel ski)
    {
        if (string.IsNullOrWhiteSpace(ski.Brand) || string.IsNullOrWhiteSpace(ski.Model))
            return BadRequest("Brand и Model обязательны");

        // Если передан StoneGrindId — проверяем, что он существует
        if (ski.StoneGrindId.HasValue)
        {
            var exists = await _context.StoneGrinds.AnyAsync(sg => sg.Id == ski.StoneGrindId);
            if (!exists) return BadRequest($"StoneGrindId {ski.StoneGrindId} не найден");
        }


if (!string.IsNullOrWhiteSpace(ski.StoneGrindName) && !ski.StoneGrindId.HasValue)
{
    var grind = await _context.StoneGrinds
        .FirstOrDefaultAsync(g => g.Name.ToLower() == ski.StoneGrindName.ToLower());
    if (grind != null)
        ski.StoneGrindId = grind.Id;
}

        _context.SkiModels.Add(ski);
        await _context.SaveChangesAsync();

        // Создаём первую запись в истории штайншлифтов, если он указан
        if (ski.StoneGrindId.HasValue)
        {
            var history = new SkiGrindHistory
            {
                SkiId = ski.Id,
                StoneGrindId = ski.StoneGrindId.Value,
                FromDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Comment = "Первоначальный шлифт"
            };
            _context.SkiGrindHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = ski.Id }, ski);
    }

    // PUT: api/Skis/5 — обновить пару
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SkiModel updated)
    {
        var ski = await _context.SkiModels.FindAsync(id);
        if (ski == null) return NotFound();

        // Обновляем поля
        ski.Brand = updated.Brand;
        ski.Model = updated.Model;
        ski.Year = updated.Year;
        ski.Style = updated.Style;
        ski.Length = updated.Length;
        ski.Profile = updated.Profile;
        ski.ProfileTempMin = updated.ProfileTempMin;
        ski.ProfileTempMax = updated.ProfileTempMax;
        ski.StiffnessValue = updated.StiffnessValue;
        ski.StiffnessLabel = updated.StiffnessLabel;
        ski.CamberHeightMm = updated.CamberHeightMm;
        ski.HasSkin = updated.HasSkin;
        ski.Notes = updated.Notes;
        ski.PersonalNotes = updated.PersonalNotes;

        // Если сменился штайншлифт — обновляем и историю
        if (ski.StoneGrindId != updated.StoneGrindId)
        {
            // Закрываем старую запись истории
            var oldHistory = await _context.SkiGrindHistories
                .Where(h => h.SkiId == id && h.ToDate == null)
                .FirstOrDefaultAsync();
            if (oldHistory != null)
                oldHistory.ToDate = DateOnly.FromDateTime(DateTime.UtcNow);

            ski.StoneGrindId = updated.StoneGrindId;

            // Создаём новую запись истории
            if (updated.StoneGrindId.HasValue)
            {
                _context.SkiGrindHistories.Add(new SkiGrindHistory
                {
                    SkiId = id,
                    StoneGrindId = updated.StoneGrindId.Value,
                    FromDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Comment = "Смена шлифта"
                });
            }
        }

if (!string.IsNullOrWhiteSpace(updated.StoneGrindName) && !updated.StoneGrindId.HasValue)
{
    var grind = await _context.StoneGrinds
        .FirstOrDefaultAsync(g => g.Name.ToLower() == updated.StoneGrindName.ToLower());
    if (grind != null)
        ski.StoneGrindId = grind.Id;
}
ski.StoneGrindName = updated.StoneGrindName;

        await _context.SaveChangesAsync();
        return Ok(ski);
    }

    // DELETE: api/Skis/5 — удалить пару (история удалится каскадом)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ski = await _context.SkiModels.FindAsync(id);
        if (ski == null) return NotFound();

        _context.SkiModels.Remove(ski);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // GET: api/Skis/5/history — история смены шлифтов
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var exists = await _context.SkiModels.AnyAsync(s => s.Id == id);
        if (!exists) return NotFound();

        var history = await _context.SkiGrindHistories
            .Where(h => h.SkiId == id)
            .Include(h => h.StoneGrind)
            .OrderByDescending(h => h.FromDate)
            .ToListAsync();

        return Ok(history);
    }
}

