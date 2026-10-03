namespace SkiApi.Models;

public class Wax
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Glide / Grip / Powder
    public string Type { get; set; } = string.Empty;     // Base / Finish / Universal
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public double HumidityMin { get; set; }
    public double HumidityMax { get; set; }

// Типы снега (8 boolean-колонок)
public bool FreshDry { get; set; }
public bool FreshWet { get; set; }
public bool OldDry { get; set; }
public bool OldWet { get; set; }
public bool TransformedDry { get; set; }
public bool TransformedWet { get; set; }
public bool IceDry { get; set; }
public bool IceWet { get; set; }

    public string TrackType { get; set; } = string.Empty; // Prepared / Unprepared / All
    public string? Notes { get; set; }
    public string? Warnings {get; set; }
}

