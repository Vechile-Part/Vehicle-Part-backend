using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task<Appointment> AddAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<PartRequest> AddPartRequestAsync(PartRequest request, CancellationToken cancellationToken = default);
    Task<ServiceReview> AddServiceReviewAsync(ServiceReview review, CancellationToken cancellationToken = default);
    Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
}
