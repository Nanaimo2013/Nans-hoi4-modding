namespace NansHoi4Tool.Models;

public class ProjectData
{
    public List<FocusTreeData> FocusTrees { get; set; } = new() { new FocusTreeData { TreeId = "generic", IsGeneric = true } };
    public EventsData Events { get; set; } = new();
    public IdeasData Ideas { get; set; } = new();
    public List<TechData> TechCategories { get; set; } = new() { new TechData { CategoryId = "infantry" } };
    public DecisionsData Decisions { get; set; } = new();
    public List<CountryData> Countries { get; set; } = new();
    public LocalisationData Localisation { get; set; } = new();
    public UnitsData Units { get; set; } = new();

    // Back-compat shims (used by exporter/importer) — point to first items
    [Newtonsoft.Json.JsonIgnore]
    public FocusTreeData FocusTree
    {
        get => FocusTrees.Count > 0 ? FocusTrees[0] : new FocusTreeData();
        set { if (FocusTrees.Count > 0) FocusTrees[0] = value; else FocusTrees.Add(value); }
    }
    [Newtonsoft.Json.JsonIgnore]
    public TechData Technologies
    {
        get => TechCategories.Count > 0 ? TechCategories[0] : new TechData();
        set { if (TechCategories.Count > 0) TechCategories[0] = value; else TechCategories.Add(value); }
    }
    [Newtonsoft.Json.JsonIgnore]
    public CountryData Country
    {
        get => Countries.Count > 0 ? Countries[0] : new CountryData();
        set { if (Countries.Count > 0) Countries[0] = value; else Countries.Add(value); }
    }
}

// ── Focus Tree ───────────────────────────────────────────────
public class FocusTreeData
{
    public string TreeId { get; set; } = "generic";
    public string CountryTag { get; set; } = string.Empty;
    public bool Continuous { get; set; }
    public bool IsGeneric { get; set; } = true;
    public List<FocusNodeDto> Nodes { get; set; } = new();
}

public class FocusNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "GFX_goal_generic_political_pressure";
    public int Cost { get; set; } = 70;
    public double X { get; set; }
    public double Y { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Prerequisites { get; set; } = new();
    public List<string> MutuallyExclusive { get; set; } = new();
    public string CompletionReward { get; set; } = string.Empty;
    public string Available { get; set; } = string.Empty;
    public bool CancelIfInvalid { get; set; } = true;
    public bool ContinueIfInvalid { get; set; }
    public bool AvailableIfCapitulated { get; set; }
}

// ── Events ───────────────────────────────────────────────────
public class EventsData
{
    public string Namespace { get; set; } = "generic";
    public List<EventDto> Events { get; set; } = new();
}

public class EventDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Picture { get; set; } = "GFX_report_event_generic_read_write";
    public string Type { get; set; } = "country_event";
    public bool IsTriggeredOnly { get; set; } = true;
    public bool Hidden { get; set; }
    public bool FireOnlyOnce { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public string MeanTimeToHappen { get; set; } = string.Empty;
    public List<EventOptionDto> Options { get; set; } = new();
}

public class EventOptionDto
{
    public string Name { get; set; } = "Option A";
    public string Trigger { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
}

// ── Ideas ────────────────────────────────────────────────────
public class IdeasData
{
    public List<IdeaDto> Ideas { get; set; } = new();
}

public class IdeaDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Picture { get; set; } = "GFX_idea_generic_political_advisor";
    public string Slot { get; set; } = "political_advisor";
    public string IdeaToken { get; set; } = string.Empty;
    public bool AllowedCivilWar { get; set; } = true;
    public string Available { get; set; } = string.Empty;
    public List<ModifierDto> Modifiers { get; set; } = new();
}

public class ModifierDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = "0.1";
}

// ── Technologies ─────────────────────────────────────────────
public class TechData
{
    public string CategoryId { get; set; } = "infantry";
    public List<TechnologyDto> Technologies { get; set; } = new();
}

public class TechnologyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ResearchCost { get; set; } = 1;
    public int StartYear { get; set; } = 1936;
    public string Folder { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public List<string> Prerequisites { get; set; } = new();
    public List<ModifierDto> Bonuses { get; set; } = new();
}

// ── Decisions ────────────────────────────────────────────────
public class DecisionsData
{
    public List<DecisionCategoryDto> Categories { get; set; } = new();
}

public class DecisionCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<DecisionDto> Decisions { get; set; } = new();
}

public class DecisionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "GFX_decision_generic_political_discourse";
    public int CostPoliticalPower { get; set; } = 50;
    public int DaysReDo { get; set; }
    public int DaysRemove { get; set; }
    public string Available { get; set; } = string.Empty;
    public string Visible { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string RemoveEffect { get; set; } = string.Empty;
}

// ── Country ──────────────────────────────────────────────────
public class CountryData
{
    public string Tag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Adjective { get; set; } = string.Empty;
    public string FlagPath { get; set; } = string.Empty;
    public string Ideology { get; set; } = "neutrality";
    public string GraphicalCulture { get; set; } = "western_european_gfx";
    public string GraphicalCulture2d { get; set; } = "western_european_2d";
    public int CapitalState { get; set; } = 1;
    public double ColorR { get; set; } = 0.5;
    public double ColorG { get; set; } = 0.5;
    public double ColorB { get; set; } = 0.5;
    public string StartingTechnology { get; set; } = string.Empty;
    public string OobPath { get; set; } = string.Empty;
}

// ── Localisation ─────────────────────────────────────────────
public class LocalisationData
{
    public List<LocalisationEntryDto> Entries { get; set; } = new();
}

public class LocalisationEntryDto
{
    public string Key { get; set; } = string.Empty;
    public string English { get; set; } = string.Empty;
    public string French { get; set; } = string.Empty;
    public string German { get; set; } = string.Empty;
    public string Spanish { get; set; } = string.Empty;
    public string Russian { get; set; } = string.Empty;
    public string Polish { get; set; } = string.Empty;
}

// ── Units ────────────────────────────────────────────────────
public class UnitsData
{
    public List<UnitDto> Units { get; set; } = new();
}

public class UnitDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string Type { get; set; } = "land";
    public string Sprite { get; set; } = string.Empty;
    public string MapIconCategory { get; set; } = "infantry";
    public int Priority { get; set; } = 600;
    public bool IsResizable { get; set; }
    public List<ModifierDto> Stats { get; set; } = new();
}
