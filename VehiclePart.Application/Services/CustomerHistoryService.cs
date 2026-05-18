using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace VehiclePart.Application.Services;

public class CustomerHistoryService(ICustomerHistoryRepository repository) : ICustomerHistoryService
{
    public async Task<CustomerHistoryDto?> GetCustomerHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await repository.GetCustomerHistoryAsync(
            customerId,
            includeAppointmentsAndReviews: true,
            cancellationToken);
    }
}