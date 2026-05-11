using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface IStaffRepository
{
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);


    Task<SalesInvoice> AddSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken = default);
    Task<SalesInvoiceItem> AddSalesInvoiceItemAsync(SalesInvoiceItem item, CancellationToken cancellationToken = default);
    Task<SalesInvoice?> GetSalesInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoiceItem>> GetSalesInvoiceItemsAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Part?> GetPartByIdAsync(Guid partId, CancellationToken cancellationToken = default);
    Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically decreases stock if enough quantity is available (single UPDATE). Returns rows affected (0 or 1).
    /// </summary>
    Task<int> TryDecrementPartStockAsync(Guid partId, int quantity, CancellationToken cancellationToken = default);

    Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchCustomersAsync(string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
}
