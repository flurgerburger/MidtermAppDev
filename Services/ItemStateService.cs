namespace midterm.Services;

using midterm.Models;

/// <summary>
/// Centralized in-memory reactive state management service.
/// Provides CRUD operations and reactive notifications to update subscribed components.
/// </summary>
public class ItemStateService
{
    private readonly List<GenericItem> _items = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event triggered whenever the item collection is mutated (added, updated, deleted, or reset).
    /// Subscribing components can call StateHasChanged() in response.
    /// </summary>
    public event Action? OnChange;

    public ItemStateService()
    {
        SeedInitialData();
    }

    /// <summary>
    /// Returns all records sorted by date descending.
    /// </summary>
    public IReadOnlyList<GenericItem> GetAll()
    {
        lock (_lock)
        {
            return _items.OrderByDescending(i => i.Date).Select(i => i.Clone()).ToList();
        }
    }

    /// <summary>
    /// Retrieves a single record by its unique identifier.
    /// </summary>
    public GenericItem? GetById(Guid id)
    {
        lock (_lock)
        {
            return _items.FirstOrDefault(i => i.Id == id)?.Clone();
        }
    }

    /// <summary>
    /// Adds a new record and triggers reactive UI notifications.
    /// </summary>
    public void Add(GenericItem item)
    {
        if (item == null) return;

        lock (_lock)
        {
            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }
            _items.Add(item.Clone());
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Updates an existing record by Id and triggers reactive UI notifications.
    /// </summary>
    public bool Update(GenericItem item)
    {
        if (item == null) return false;

        bool updated = false;
        lock (_lock)
        {
            var index = _items.FindIndex(i => i.Id == item.Id);
            if (index >= 0)
            {
                _items[index] = item.Clone();
                updated = true;
            }
        }

        if (updated)
        {
            NotifyStateChanged();
        }

        return updated;
    }

    /// <summary>
    /// Deletes a record by its unique identifier and triggers reactive UI notifications.
    /// </summary>
    public bool Delete(Guid id)
    {
        bool removed = false;
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _items.Remove(item);
                removed = true;
            }
        }

        if (removed)
        {
            NotifyStateChanged();
        }

        return removed;
    }

    /// <summary>
    /// Resets the store back to initial dummy data.
    /// </summary>
    public void ResetToDefaults()
    {
        lock (_lock)
        {
            _items.Clear();
            SeedInitialData();
        }

        NotifyStateChanged();
    }

    // --- Computed aggregate metrics for Dashboard and reporting ---

    public int TotalCount
    {
        get
        {
            lock (_lock) return _items.Count;
        }
    }

    public decimal TotalValue
    {
        get
        {
            lock (_lock) return _items.Sum(i => i.Value);
        }
    }

    public int ActiveCount
    {
        get
        {
            lock (_lock) return _items.Count(i => i.Status == "Active");
        }
    }

    public int PendingCount
    {
        get
        {
            lock (_lock) return _items.Count(i => i.Status == "Pending" || i.Status == "In Progress");
        }
    }

    public int CompletedCount
    {
        get
        {
            lock (_lock) return _items.Count(i => i.Status == "Completed");
        }
    }

    public decimal AverageValue
    {
        get
        {
            lock (_lock) return _items.Count > 0 ? _items.Average(i => i.Value) : 0;
        }
    }

    public IReadOnlyList<string> Categories
    {
        get
        {
            lock (_lock) return _items.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private void SeedInitialData()
    {
        var sampleData = new List<GenericItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "System Architecture Review",
                Description = "Conduct thorough review of modular design and interface boundaries.",
                Category = "Technology",
                Value = 1250.00m,
                Status = "Completed",
                Date = DateTime.Today.AddDays(-6)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Resource Allocation Assessment",
                Description = "Evaluate workload distribution and capacity limits across units.",
                Category = "Operations",
                Value = 3400.50m,
                Status = "Active",
                Date = DateTime.Today.AddDays(-4)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Data Pipeline Verification",
                Description = "Verify ingestion throughput and data transform integrity scripts.",
                Category = "Technology",
                Value = 750.25m,
                Status = "Active",
                Date = DateTime.Today.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Inventory Discrepancy Audit",
                Description = "Cross-reference scheduled consignments with recorded warehouse logs.",
                Category = "Logistics",
                Value = 2100.00m,
                Status = "Pending",
                Date = DateTime.Today.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Compliance Standards Update",
                Description = "Review operational guidelines against updated regulatory protocols.",
                Category = "Quality Assurance",
                Value = 950.00m,
                Status = "In Progress",
                Date = DateTime.Today.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Experimental Test Bench Alpha",
                Description = "Execute automated telemetry benchmarks under peak stress load.",
                Category = "Research",
                Value = 4800.75m,
                Status = "Active",
                Date = DateTime.Today
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Dispatch Route Optimization",
                Description = "Simulate alternative distribution paths to minimize transit overhead.",
                Category = "Logistics",
                Value = 1620.00m,
                Status = "Completed",
                Date = DateTime.Today.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "User Experience Walkthrough",
                Description = "Audit interactive navigation and component accessibility conformance.",
                Category = "General",
                Value = 540.00m,
                Status = "Archived",
                Date = DateTime.Today.AddDays(-10)
            }
        };

        _items.AddRange(sampleData);
    }
}
