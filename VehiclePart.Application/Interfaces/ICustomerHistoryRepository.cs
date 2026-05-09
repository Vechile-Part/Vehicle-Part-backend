using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface ICustomerHistoryRepository
{
    Task<CustomerHistoryDto?> GetCustomerHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
}