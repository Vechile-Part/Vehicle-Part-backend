namespace VehiclePart.Application.Common;

public static class NepalClock
{
    private static readonly TimeZoneInfo Zone = ResolveZone();

    private static TimeZoneInfo ResolveZone()
    {
        foreach (var id in new[] { "Asia/Kathmandu", "Nepal Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException(
            "Nepal time zone not found. Expected 'Asia/Kathmandu' or 'Nepal Standard Time'.");
    }

    public static DateTime NowLocal() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

    public static DateTime LocalDateToUtcStart(DateTime localDate)
    {
        var local = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, Zone);
    }

    public static DateTime TodayStartUtc => LocalDateToUtcStart(NowLocal());

    public static DateTime MonthStartUtc =>
        LocalDateToUtcStart(new DateTime(NowLocal().Year, NowLocal().Month, 1));

    public static DateTime YearStartUtc =>
        LocalDateToUtcStart(new DateTime(NowLocal().Year, 1, 1));

    public static (DateTime StartUtc, DateTime EndUtc) DayRangeUtc(DateTime nepalLocalDate)
    {
        var start = LocalDateToUtcStart(nepalLocalDate);
        var end = LocalDateToUtcStart(nepalLocalDate.Date.AddDays(1));
        return (start, end);
    }

    public static DateTime LocalDateTimeToUtc(int year, int month, int day, int hour, int minute) =>
        TimeZoneInfo.ConvertTimeToUtc(
            new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified),
            Zone);

    public static DateTime LocalSlotToUtc(int year, int month, int day, string slotLabel)
    {
        if (!TryParseTimeSlotLabel(slotLabel, out var hour, out var minute))
            throw new ArgumentException($"Invalid time slot label: {slotLabel}", nameof(slotLabel));

        return LocalDateTimeToUtc(year, month, day, hour, minute);
    }

    public static bool TryParseTimeSlotLabel(string label, out int hour24, out int minute)
    {
        hour24 = 0;
        minute = 0;

        var trimmed = (label ?? string.Empty).Trim();
        var space = trimmed.LastIndexOf(' ');
        if (space <= 0 || space >= trimmed.Length - 1)
            return false;

        var timePart = trimmed[..space].Trim();
        var meridiem = trimmed[(space + 1)..].Trim().ToUpperInvariant();
        if (meridiem is not "AM" and not "PM")
            return false;

        var colon = timePart.IndexOf(':');
        if (colon <= 0 || colon >= timePart.Length - 1)
            return false;

        if (!int.TryParse(timePart[..colon], out var hour12) || !int.TryParse(timePart[(colon + 1)..], out minute))
            return false;

        if (hour12 is < 1 or > 12 || minute is < 0 or > 59)
            return false;

        hour24 = hour12 % 12;
        if (meridiem == "PM")
            hour24 += 12;

        return true;
    }

    public static string FormatLocalDateTime(DateTime utc, string format = "dd MMM yyyy, hh:mm tt")
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(NormalizeToUtc(utc), Zone);
        return $"{local.ToString(format)} NPT";
    }

    public static string FormatTimeSlotLabel(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(
            NormalizeToUtc(utc),
            Zone);

        var hour12 = local.Hour % 12;
        if (hour12 == 0)
            hour12 = 12;

        var meridiem = local.Hour < 12 ? "AM" : "PM";
        return $"{hour12:00}:{local.Minute:00} {meridiem}";
    }

    private static DateTime NormalizeToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
}
