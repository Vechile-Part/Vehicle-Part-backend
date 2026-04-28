using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    Task<Appointment> AddAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<PartRequest> AddPartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default);
    Task<ServiceReview> AddServiceReviewAsync(ServiceReview review, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartRequest>> GetPartRequestsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceReview>> GetServiceReviewsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<SalesInvoice?> GetSalesInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
}
