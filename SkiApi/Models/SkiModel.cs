namespace SkiApi.Models;

public class SkiModel
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Style { get; set; } = string.Empty;   // Free / Classic
    public int? Length { get; set; }                     // 192 см

    public string? Profile { get; set; }                 // s2, cold, blue, 61k
    public double? ProfileTempMin { get; set; }
    public double? ProfileTempMax { get; set; }

    public double? StiffnessValue { get; set; }          // 100, 106, 112
    public string? StiffnessLabel { get; set; }          // FA / MF / flex
    public double? CamberHeightMm { get; set; }          // 2.6, 2.38

    public bool HasSkin { get; set; }                    // камус?

    // FK на текущий штайншлифт

    public string? StoneGrindName { get; set; } 
    public int? StoneGrindId { get; set; }
    public StoneGrind? StoneGrind { get; set; }

    public string? Notes { get; set; }                   // заводские
    public string? PersonalNotes { get; set; }           // наблюдения спортсмена
}
