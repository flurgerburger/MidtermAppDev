namespace midterm.Models;

/// <summary>
/// Domain-agnostic flexible data model for lab exams and generic templates.
/// </summary>
public class GenericItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public decimal Value { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Predefined generic categories suitable for any domain scenario.
    /// </summary>
    public static readonly string[] DefaultCategories =
    [
        "Operations",
        "Technology",
        "Logistics",
        "Research",
        "Quality Assurance",
        "General"
    ];

    /// <summary>
    /// Predefined generic lifecycle statuses.
    /// </summary>
    public static readonly string[] DefaultStatuses =
    [
        "Active",
        "Pending",
        "In Progress",
        "Completed",
        "Archived"
    ];

    /// <summary>
    /// Creates a deep copy of the item to prevent unwanted direct state mutation.
    /// </summary>
    public GenericItem Clone() => new()
    {
        Id = Id,
        Title = Title,
        Description = Description,
        Category = Category,
        Value = Value,
        Status = Status,
        Date = Date
    };
}
