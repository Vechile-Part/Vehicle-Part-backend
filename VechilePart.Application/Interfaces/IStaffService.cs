using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface IStaffService
{
    Task RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<Guid> CreateSalesInvoiceAsync(SalesInvoiceCreateDto dto, CancellationToken cancellationToken = default);
    Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> SearchCustomersAsync(CustomerSearchDto dto, CancellationToken cancellationToken = default);
    Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
