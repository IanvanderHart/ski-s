namespace SkiApi.Models;

public class StoneGrind
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? TempMin { get; set; }
    public double? TempMax { get; set; }
    public string? SnowTypes { get; set; }   // "FreshDry,FreshWet,OldWet"
    public string? TrackType { get; set; }   // Prepared / Unprepared / All
    public string? Notes { get; set; }
}

