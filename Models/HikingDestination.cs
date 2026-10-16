namespace midterm.Models;

/// <summary>
/// Represents a local hiking trail and recreational peak in Cebu.
/// </summary>
public class HikingDestination
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TrailName { get; set; } = string.Empty;
    public string LocalMunicipality { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Moderate"; // Easy, Moderate, Expert
    public int ElevationMeters { get; set; }
    public string Status { get; set; } = "Open"; // Open, Closed, Weather Advisory
    public decimal PermitFee { get; set; }

    // Additional thematic metadata for dialogs and detail screens
    public string Description { get; set; } = string.Empty;
    public double DistanceKm { get; set; } = 4.5;
    public double Rating { get; set; } = 4.8;
    public int EstimatedMinutes { get; set; } = 90;
    public string SafetyNotes { get; set; } = "Bring at least 1.5L of water, wear proper trail shoes, and check weather alerts before departure.";

    /// <summary>
    /// Standard difficulty tiers.
    /// </summary>
    public static readonly string[] Difficulties =
    [
        "Easy",
        "Moderate",
        "Expert"
    ];

    /// <summary>
    /// Current trail access and weather safety statuses.
    /// </summary>
    public static readonly string[] Statuses =
    [
        "Open",
        "Weather Advisory",
        "Closed"
    ];

    /// <summary>
    /// Actual Cebu municipalities and cities renowned for hiking trails.
    /// </summary>
    public static readonly string[] Municipalities =
    [
        "Cebu City",
        "Dalaguete",
        "Balamban",
        "Naga",
        "Carmen",
        "Danao",
        "Carcar",
        "Cordova",
        "San Fernando"
    ];

    /// <summary>
    /// Creates a deep copy of the destination to prevent unintended state mutation before saving.
    /// </summary>
    public HikingDestination Clone() => new()
    {
        Id = Id,
        TrailName = TrailName,
        LocalMunicipality = LocalMunicipality,
        Difficulty = Difficulty,
        ElevationMeters = ElevationMeters,
        Status = Status,
        PermitFee = PermitFee,
        Description = Description,
        DistanceKm = DistanceKm,
        Rating = Rating,
        EstimatedMinutes = EstimatedMinutes,
        SafetyNotes = SafetyNotes
    };
}
