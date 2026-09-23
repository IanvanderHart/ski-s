namespace SkiApi.Models;

public class SkiGrindHistory
{
    public int Id { get; set; }

    public int SkiId { get; set; }
    public SkiModel? Ski { get; set; }

    public int StoneGrindId { get; set; }
    public StoneGrind? StoneGrind { get; set; }

    public DateOnly FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Comment { get; set; }
}

