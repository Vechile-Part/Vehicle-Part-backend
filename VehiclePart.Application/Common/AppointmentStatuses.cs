namespace VehiclePart.Application.Common;

public static class AppointmentStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Completed = "Completed";
    public const string NoShow = "NoShow";
    public const string Cancelled = "Cancelled";

    private static readonly HashSet<string> AllowedSet = new(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Confirmed,
        Completed,
        NoShow,
        Cancelled,
    };

    public static IReadOnlySet<string> Allowed => AllowedSet;

    public static string Normalize(string? status)
    {
        var value = (status ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Status is required.", nameof(status));

        if (value.Equals("no show", StringComparison.OrdinalIgnoreCase)
            || value.Equals("no-show", StringComparison.OrdinalIgnoreCase)
            || value.Equals("noshow", StringComparison.OrdinalIgnoreCase))
        {
            return NoShow;
        }

        foreach (var allowed in AllowedSet)
        {
            if (value.Equals(allowed, StringComparison.OrdinalIgnoreCase))
                return allowed;
        }

        throw new ArgumentException(
            "Status must be Pending, Confirmed, Completed, NoShow, or Cancelled.",
            nameof(status));
    }

    public static bool BlocksTimeSlot(string status) =>
        string.Equals(status, Pending, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Confirmed, StringComparison.OrdinalIgnoreCase);

    public static bool CanBeReviewed(string status) =>
        string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase);

    public static bool IsUpcoming(string status, DateTime appointmentDateUtc)
    {
        if (!BlocksTimeSlot(status))
            return false;

        return appointmentDateUtc >= DateTime.UtcNow;
    }
}
