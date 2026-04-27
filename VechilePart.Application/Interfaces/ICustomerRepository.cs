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
    Task<SalesInvoice?> GetSalesInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
}
