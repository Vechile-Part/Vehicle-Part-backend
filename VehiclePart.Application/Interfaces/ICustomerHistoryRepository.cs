using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface ICustomerHistoryRepository
{
    Task<CustomerHistoryDto?> GetCustomerHistoryAsync(
        Guid customerId,
        bool includeAppointmentsAndReviews = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerPartPurchaseLineDto>> GetCustomerPartPurchasesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}
