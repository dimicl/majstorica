namespace backend.Domain.Enums;

/// <summary>Kategorija majstora – jedna po majstoru. Vrednosti 1–6: kratko u bazi i API-ju.</summary>
public enum MasterCategory
{
    Elektricar = 1,
    Vodoinstalater = 2,
    Keramicar = 3,
    MajstorZaSve = 4,
    Moler = 5,
    Stolar = 6
}

public static class MasterCategoryDisplay
{
    private static readonly Dictionary<MasterCategory, string> Names = new()
    {
        [MasterCategory.Elektricar] = "Električar",
        [MasterCategory.Vodoinstalater] = "Vodoinstalater",
        [MasterCategory.Keramicar] = "Keramičar",
        [MasterCategory.MajstorZaSve] = "Majstor za sve",
        [MasterCategory.Moler] = "Moler",
        [MasterCategory.Stolar] = "Stolar"
    };

    public static string ToDisplayName(MasterCategory category) => Names.GetValueOrDefault(category, category.ToString());

    public static MasterCategory? FromDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName)) return null;
        foreach (var (cat, name) in Names)
            if (string.Equals(name, displayName.Trim(), StringComparison.OrdinalIgnoreCase))
                return cat;
        return null;
    }

    public static MasterCategory? FromValue(int? value)
    {
        if (value is null or < 1 or > 6) return null;
        return (MasterCategory)value.Value;
    }
}
