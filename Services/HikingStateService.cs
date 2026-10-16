namespace midterm.Services;

using midterm.Models;

/// <summary>
/// Reactive in-memory state management service for the complete Cebu hiking dataset.
/// </summary>
public class HikingStateService
{
    private readonly List<HikingDestination> _trails = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event triggered when trails are added, modified, removed, or reset.
    /// </summary>
    public event Action? OnChange;

    public HikingStateService()
    {
        SeedInitialTrails();
    }

    /// <summary>
    /// Retrieves all hiking trails.
    /// </summary>
    public IReadOnlyList<HikingDestination> GetAll()
    {
        lock (_lock)
        {
            return _trails.Select(t => t.Clone()).ToList();
        }
    }

    /// <summary>
    /// Retrieves a specific trail by ID.
    /// </summary>
    public HikingDestination? GetById(Guid id)
    {
        lock (_lock)
        {
            return _trails.FirstOrDefault(t => t.Id == id)?.Clone();
        }
    }

    /// <summary>
    /// Adds a new hiking destination and notifies subscribers.
    /// </summary>
    public void Add(HikingDestination trail)
    {
        if (trail == null) return;

        lock (_lock)
        {
            if (trail.Id == Guid.Empty)
            {
                trail.Id = Guid.NewGuid();
            }
            _trails.Add(trail.Clone());
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Updates an existing trail record and notifies subscribers.
    /// </summary>
    public bool Update(HikingDestination trail)
    {
        if (trail == null) return false;

        bool updated = false;
        lock (_lock)
        {
            var index = _trails.FindIndex(t => t.Id == trail.Id);
            if (index >= 0)
            {
                _trails[index] = trail.Clone();
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
    /// Deletes a trail by ID and notifies subscribers.
    /// </summary>
    public bool Delete(Guid id)
    {
        bool removed = false;
        lock (_lock)
        {
            var trail = _trails.FirstOrDefault(t => t.Id == id);
            if (trail != null)
            {
                _trails.Remove(trail);
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
    /// Restores default comprehensive Cebu trail dataset across all 3 tiers.
    /// </summary>
    public void ResetToDefaults()
    {
        lock (_lock)
        {
            _trails.Clear();
            SeedInitialTrails();
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Filters destinations based on search query, municipality, difficulty, and status.
    /// </summary>
    public IEnumerable<HikingDestination> Filter(string? search, string? municipality, string? difficulty, string? status)
    {
        lock (_lock)
        {
            var query = _trails.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(municipality))
            {
                query = query.Where(t => string.Equals(t.LocalMunicipality, municipality, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                query = query.Where(t => string.Equals(t.Difficulty, difficulty, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t =>
                    t.TrailName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    t.LocalMunicipality.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    t.Difficulty.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    t.Status.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    t.ElevationMeters.ToString().Contains(term)
                );
            }

            return query.Select(t => t.Clone()).ToList();
        }
    }

    /// <summary>
    /// Suggests a random open trail matching optional municipality and difficulty preferences.
    /// </summary>
    public HikingDestination? GetRandomSuggestion(string? municipality = null, string? difficulty = null)
    {
        lock (_lock)
        {
            var eligible = _trails.Where(t => t.Status == "Open");

            if (!string.IsNullOrWhiteSpace(municipality))
                eligible = eligible.Where(t => string.Equals(t.LocalMunicipality, municipality, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(difficulty))
                eligible = eligible.Where(t => string.Equals(t.Difficulty, difficulty, StringComparison.OrdinalIgnoreCase));

            var list = eligible.ToList();
            if (list.Count == 0) return null;

            var random = new Random();
            return list[random.Next(list.Count)].Clone();
        }
    }

    // --- Computed Aggregate Metrics ---

    public int TotalTrails
    {
        get
        {
            lock (_lock) return _trails.Count;
        }
    }

    public int OpenTrails
    {
        get
        {
            lock (_lock) return _trails.Count(t => t.Status == "Open");
        }
    }

    public int AdvisoryTrails
    {
        get
        {
            lock (_lock) return _trails.Count(t => t.Status == "Weather Advisory");
        }
    }

    public int ClosedTrails
    {
        get
        {
            lock (_lock) return _trails.Count(t => t.Status == "Closed");
        }
    }

    public int AverageElevation
    {
        get
        {
            lock (_lock) return _trails.Count > 0 ? (int)_trails.Average(t => t.ElevationMeters) : 0;
        }
    }

    public decimal AveragePermitFee
    {
        get
        {
            lock (_lock) return _trails.Count > 0 ? _trails.Average(t => t.PermitFee) : 0;
        }
    }

    public IReadOnlyList<string> Municipalities
    {
        get
        {
            lock (_lock) return _trails.Select(t => t.LocalMunicipality).Distinct().OrderBy(m => m).ToList();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private void SeedInitialTrails()
    {
        var sampleTrails = new List<HikingDestination>
        {
            // ==========================================
            // TIER 1: EASY & BEGINNER TRAILS
            // ==========================================
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "\"Starbuks\" Rest Stop Trail",
                LocalMunicipality = "Cebu City",
                Difficulty = "Easy",
                ElevationMeters = 320,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "A casual uphill hike leading to the famous local mountain shack overlooking Metro Cebu. Popular for quick morning strolls, coffee breaks, and community fitness.",
                DistanceKm = 2.8,
                Rating = 4.8,
                EstimatedMinutes = 50,
                SafetyNotes = "Wear comfortable walking shoes; path can get dusty in dry season or slippery after light showers."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "CCLEX Bridge Pedestrian Crossing",
                LocalMunicipality = "Cordova",
                Difficulty = "Easy",
                ElevationMeters = 65,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "A scenic, fully paved incline walk over the Mactan Channel on the Philippines' longest cable-stayed bridge. Offers refreshing sea breezes and coastal panoramas.",
                DistanceKm = 8.9,
                Rating = 4.6,
                EstimatedMinutes = 110,
                SafetyNotes = "High midday sun exposure with no shade; bring sunscreen, sunglasses, and adequate drinking water."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Buhisan Toong Tree Tunnel",
                LocalMunicipality = "Cebu City",
                Difficulty = "Easy",
                ElevationMeters = 210,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "Gentle, shady walk sheltered by an arching canopy of lush tropical trees, connecting directly into the greater Buhisan watershed trail network.",
                DistanceKm = 3.5,
                Rating = 4.7,
                EstimatedMinutes = 60,
                SafetyNotes = "Tree roots can be tripping hazards; insect repellent recommended."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Osmeña Peak",
                LocalMunicipality = "Dalaguete",
                Difficulty = "Easy",
                ElevationMeters = 1013,
                Status = "Open",
                PermitFee = 50.00m,
                Description = "Cebu’s highest peak, reachable via a brief 20-minute walk from the base camp. Offers iconic vistas of jagged, chocolate-hill-like karst formations and Tañon Strait.",
                DistanceKm = 2.2,
                Rating = 4.9,
                EstimatedMinutes = 40,
                SafetyNotes = "Strong summit gusts and chilly highland winds; a light windbreaker is recommended."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Mount Naupa",
                LocalMunicipality = "Naga",
                Difficulty = "Easy",
                ElevationMeters = 584,
                Status = "Open",
                PermitFee = 30.00m,
                Description = "Short, open grassland ridge climb in the rolling hills of Naga. Renowned for relaxing hillock panoramas, campsite stargazing, and golden sunrise views.",
                DistanceKm = 3.6,
                Rating = 4.8,
                EstimatedMinutes = 60,
                SafetyNotes = "Exposed grassland with minimal canopy cover; start early morning or late afternoon to avoid heat."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Mount Mago",
                LocalMunicipality = "Carmen",
                Difficulty = "Easy",
                ElevationMeters = 755,
                Status = "Open",
                PermitFee = 50.00m,
                Description = "Rolling open grassy ridges where the borders of Carmen, Danao, and Tuburan intersect. Features gentle undulations and panoramic 360-degree countryside vistas.",
                DistanceKm = 6.2,
                Rating = 4.7,
                EstimatedMinutes = 130,
                SafetyNotes = "Long sunny stretches across ridges; hydrate regularly and wear a wide-brim hat."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Casino Peak / Lugsangan Peak",
                LocalMunicipality = "Dalaguete",
                Difficulty = "Easy",
                ElevationMeters = 930,
                Status = "Open",
                PermitFee = 50.00m,
                Description = "A short but steep stair-step climb up jagged limestone karsts. Overlooks vegetable terrace farms and looks like a greener version of the Chocolate Hills.",
                DistanceKm = 3.0,
                Rating = 4.9,
                EstimatedMinutes = 50,
                SafetyNotes = "Sharp limestone rocks near summit; use hands carefully and avoid leaning past sheer rock edges."
            },

            // ==========================================
            // TIER 2: MODERATE & INTERMEDIATE TRAILS
            // ==========================================
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "6 Towers Trail (Amordino Route)",
                LocalMunicipality = "Cebu City",
                Difficulty = "Moderate",
                ElevationMeters = 580,
                Status = "Open",
                PermitFee = 20.00m,
                Description = "Popular forest and ridge network in Buhisan following high-voltage power lines. Provides continuous elevation changes and beautiful watershed scenery.",
                DistanceKm = 7.2,
                Rating = 4.7,
                EstimatedMinutes = 160,
                SafetyNotes = "Steep ascents and loose gravel sections; hiking poles helpful on descents."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Mount Kan-Irag Campsite / Sirao Peak",
                LocalMunicipality = "Cebu City",
                Difficulty = "Moderate",
                ElevationMeters = 780,
                Status = "Open",
                PermitFee = 50.00m,
                Description = "Standard high-elevation mountain route from the metro. Features pine groves, open breezy slopes, and a wide campsite with sweeping night views of the city.",
                DistanceKm = 7.5,
                Rating = 4.8,
                EstimatedMinutes = 180,
                SafetyNotes = "Night temperatures can drop; bring warm layers if camping or starting pre-dawn."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Manggahan Trail (Banawa)",
                LocalMunicipality = "Cebu City",
                Difficulty = "Moderate",
                ElevationMeters = 450,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "Dirt trail with exposed tree roots past the Good Shepherd convent, ascending through mango orchards and connecting into the higher Banawa mountain ridges.",
                DistanceKm = 4.8,
                Rating = 4.6,
                EstimatedMinutes = 120,
                SafetyNotes = "Steep root steps require good balance; wear trail shoes with aggressive tread."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Bocawe Peak via Pamutan",
                LocalMunicipality = "Cebu City",
                Difficulty = "Moderate",
                ElevationMeters = 690,
                Status = "Open",
                PermitFee = 20.00m,
                Description = "Immediate primitive jungle incline starting from Pamutan, climbing through dense foliage and rocky single-tracks up to Bocawe's scenic high shoulder.",
                DistanceKm = 6.5,
                Rating = 4.7,
                EstimatedMinutes = 160,
                SafetyNotes = "Thick brush and narrow paths; long trousers or compression pants recommended to avoid scratches."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Kandungaw Peak",
                LocalMunicipality = "Dalaguete",
                Difficulty = "Moderate",
                ElevationMeters = 900,
                Status = "Weather Advisory",
                PermitFee = 50.00m,
                Description = "Thrilling rock path and cave trails along Dalaguete's dramatic cliffs. Features picturesque stone towers and narrow cliffside walking ledges.",
                DistanceKm = 4.5,
                Rating = 4.9,
                EstimatedMinutes = 110,
                SafetyNotes = "High vertical drops; exercise extreme caution during gusty afternoon winds or damp conditions."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Mt. Kalatkat",
                LocalMunicipality = "Carcar",
                Difficulty = "Moderate",
                ElevationMeters = 640,
                Status = "Open",
                PermitFee = 30.00m,
                Description = "Scenic high power-tower ridge paths straddling the Carcar and Sibonga borders, characterized by rolling grassy slopes and sweeping southern Cebu views.",
                DistanceKm = 8.0,
                Rating = 4.7,
                EstimatedMinutes = 190,
                SafetyNotes = "Unshaded ridge walk; carry at least 2 liters of water and electrolytic snacks."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "San Carlos Heights to Kawa Falls",
                LocalMunicipality = "Cebu City",
                Difficulty = "Moderate",
                ElevationMeters = 480,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "Unmaintained forest path descending through riverbeds and jungle ravines down to a secluded cascade with natural pool bowls.",
                DistanceKm = 5.5,
                Rating = 4.6,
                EstimatedMinutes = 140,
                SafetyNotes = "Slippery wet river boulders; bring water-resistant footwear and dry bags for electronics."
            },

            // ==========================================
            // TIER 3: HARD & CHALLENGING ENDURANCE TRAILS
            // ==========================================
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Spartan Trail (Buhisan to Banawa)",
                LocalMunicipality = "Cebu City",
                Difficulty = "Expert",
                ElevationMeters = 820,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "Cebu's toughest core day trail passing Starbuks rest stop. A relentless series of steep climbs, technical rocky descents, and rugged river gulches.",
                DistanceKm = 12.5,
                Rating = 4.9,
                EstimatedMinutes = 330,
                SafetyNotes = "High endurance required; do not attempt alone. Carry a minimum of 2.5L water and headlamps if starting late."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "6 Towers Trail (Full Point-to-Point)",
                LocalMunicipality = "Cebu City",
                Difficulty = "Expert",
                ElevationMeters = 750,
                Status = "Open",
                PermitFee = 30.00m,
                Description = "An intense 8.8-mile endurance hike through the full Buhisan Watershed reservation. Demands serious stamina across multiple ridge climbs and valleys.",
                DistanceKm = 14.2,
                Rating = 4.8,
                EstimatedMinutes = 360,
                SafetyNotes = "Remote watershed route with no resupply stores along the main ridge; carry trail food and first-aid gear."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Bocaue Peak (\"Muffin Peak\" Main Route)",
                LocalMunicipality = "Cebu City",
                Difficulty = "Expert",
                ElevationMeters = 720,
                Status = "Weather Advisory",
                PermitFee = 20.00m,
                Description = "Grueling mountain overgrowth scramble across untamed terrain up to the unmistakable muffin-shaped summit rock. Steep, thorny, and demanding.",
                DistanceKm = 9.8,
                Rating = 4.8,
                EstimatedMinutes = 270,
                SafetyNotes = "Dense thorny undergrowth and steep muddy grades; gloves and full leg protection are strongly advised."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Budlaan Falls to Sirao Peak Traverse",
                LocalMunicipality = "Cebu City",
                Difficulty = "Expert",
                ElevationMeters = 720,
                Status = "Open",
                PermitFee = 50.00m,
                Description = "River-bouldering, cliff climbing, and mountain scaling adventure starting at Budlaan Falls gorge, concluding on the windy peaks of Sirao.",
                DistanceKm = 8.5,
                Rating = 4.9,
                EstimatedMinutes = 260,
                SafetyNotes = "Technical rock bouldering along active river canyon; flash floods possible after heavy rains. Local guide required."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Bacayan - Agsungot - Pulangbato Loop",
                LocalMunicipality = "Cebu City",
                Difficulty = "Expert",
                ElevationMeters = 860,
                Status = "Open",
                PermitFee = 0.00m,
                Description = "A massive 17-mile mountain village endurance loop through northern Cebu City hinterlands. Passes rolling farming valleys, remote chapels, and peaks.",
                DistanceKm = 27.4,
                Rating = 4.8,
                EstimatedMinutes = 480,
                SafetyNotes = "Ultramarathon training distance; plan hydration refuel stops at remote barangay sari-sari stores."
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrailName = "Mount Mauyog",
                LocalMunicipality = "Balamban",
                Difficulty = "Expert",
                ElevationMeters = 1020,
                Status = "Open",
                PermitFee = 100.00m,
                Description = "Vertical limestone rock scrambling leading to a brain-shaped stone summit protruding high above Balamban's mountain valleys. Unmatched 360-degree vistas.",
                DistanceKm = 8.0,
                Rating = 4.9,
                EstimatedMinutes = 240,
                SafetyNotes = "Knife-edge limestone scrambling on exposed cliff drops; grippy footwear, helmet, and climbing gloves recommended."
            }
        };

        _trails.AddRange(sampleTrails);
    }
}
