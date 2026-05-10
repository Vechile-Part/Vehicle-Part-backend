using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface IStaffRepository
{
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);


    Task<SalesInvoice> AddSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken = default);
    Task<SalesInvoiceItem> AddSalesInvoiceItemAsync(SalesInvoiceItem item, CancellationToken cancellationToken = default);
    Task<SalesInvoice?> GetSalesInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoiceItem>> GetSalesInvoiceItemsAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Part?> GetPartByIdAsync(Guid partId, CancellationToken cancellationToken = default);
    Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default);

    Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchCustomersAsync(string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
}
