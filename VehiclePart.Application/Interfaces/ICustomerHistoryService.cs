using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface ICustomerHistoryService
{
    Task<CustomerHistoryDto?> GetCustomerHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
}