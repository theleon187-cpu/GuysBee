using System.Globalization;

namespace GuysBee.Services;

/// <summary>Jours de la semaine d'un rendez-vous récurrent, stockés en masque de bits (0 = lundi ... 6 = dimanche).</summary>
public static class WeekDays
{
    private static readonly CultureInfo Fr = new("fr-FR");

    public static readonly DayOfWeek[] Ordered =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    public static string Name(int index) => Fr.TextInfo.ToTitleCase(Fr.DateTimeFormat.GetDayName(Ordered[index]));

    public static string ShortName(int index) => Fr.TextInfo.ToTitleCase(Fr.DateTimeFormat.GetAbbreviatedDayName(Ordered[index]).TrimEnd('.'));

    public static int ToMask(IEnumerable<int>? indexes)
    {
        var mask = 0;
        if (indexes is null) return mask;
        foreach (var i in indexes)
            if (i is >= 0 and <= 6) mask |= 1 << i;
        return mask;
    }

    public static bool Has(int mask, int index) => (mask & (1 << index)) != 0;

    public static int SortKey(int mask)
    {
        for (var i = 0; i < 7; i++) if (Has(mask, i)) return i;
        return 7;
    }

    public static DayOfWeek FirstDay(int mask) => Ordered[Math.Min(SortKey(mask), 6)];

    public static string Format(int mask)
    {
        var names = Enumerable.Range(0, 7).Where(i => Has(mask, i)).Select(i => Name(i).ToLower(Fr)).ToList();
        if (names.Count == 0) return "";
        if (names.Count == 7) return "Tous les jours";
        var text = names.Count == 1 ? names[0] : string.Join(", ", names.Take(names.Count - 1)) + " et " + names[^1];
        return Fr.TextInfo.ToTitleCase(text[..1]) + text[1..];
    }
}
