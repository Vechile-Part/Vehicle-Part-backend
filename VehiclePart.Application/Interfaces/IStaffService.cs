using VehiclePart.Application.DTOs;
namespace VehiclePart.Application.Interfaces;

public interface IStaffService
{
    Task<Guid> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default);

    // Feature 7 — sell parts & create sales invoices
    Task<SalesInvoiceResponseDto> CreateSalesInvoiceAsync(SalesInvoiceCreateDto dto, CancellationToken cancellationToken = default);
    Task<SalesInvoiceResponseDto?> GetSalesInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default);

    // Feature 10 — search by vehicle number, phone, ID or name
    Task<IReadOnlyList<object>> SearchCustomersAsync(CustomerSearchDto dto, CancellationToken cancellationToken = default);

    Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}