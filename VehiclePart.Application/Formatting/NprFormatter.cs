namespace VehiclePart.Application.Formatting;

public static class NprFormatter
{
    public static string Format(decimal amount) => $"Rs. {amount:N2}";
}
