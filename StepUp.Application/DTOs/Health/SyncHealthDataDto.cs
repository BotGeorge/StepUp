using System.Text.Json.Serialization;

namespace StepUp.Application.DTOs.Health;

public class SyncHealthDataDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
    
    [JsonPropertyName("steps")]
    public int? Steps { get; set; }
    
    [JsonPropertyName("distance")]
    public decimal? Distance { get; set; } // in kilometers
    
    [JsonPropertyName("calories")]
    public int? Calories { get; set; }
    
    [JsonPropertyName("heartRate")]
    public int? HeartRate { get; set; }
    
    [JsonPropertyName("activeMinutes")]
    public int? ActiveMinutes { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; } // "Google Fit", "Apple Health", etc.
}
