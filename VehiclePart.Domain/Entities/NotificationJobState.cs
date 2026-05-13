namespace VehiclePart.Domain.Entities;


public class NotificationJobState
{
    public const string WellKnownIdString = "00000000-0000-0000-0000-0000000000a0";

    public static readonly Guid WellKnownId = Guid.Parse(WellKnownIdString);

    public Guid Id { get; set; } = WellKnownId;

    
    public DateTime? LastLowStockDigestSentUtc { get; set; }
}
