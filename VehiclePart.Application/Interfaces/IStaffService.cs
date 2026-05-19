using VehiclePart.Application.DTOs;
namespace VehiclePart.Application.Interfaces;

public interface IStaffService
{
    Task<Guid> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default);

   
    Task<SalesInvoiceResponseDto> CreateSalesInvoiceAsync(SalesInvoiceCreateDto dto, CancellationToken cancellationToken = default);
    Task<SalesInvoiceResponseDto?> GetSalesInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoiceSummaryDto>> ListSalesInvoicesAsync(CancellationToken cancellationToken = default);

    Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default);


    Task<IReadOnlyList<object>> SearchCustomersAsync(CustomerSearchDto dto, CancellationToken cancellationToken = default);

    Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StaffAppointmentDto>> ListAppointmentsAsync(CancellationToken cancellationToken = default);

    Task UpdateAppointmentStatusAsync(
        Guid appointmentId,
        UpdateAppointmentStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<StaffProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpdateMyProfileAsync(Guid userId, UpdateStaffSelfProfileDto dto, CancellationToken cancellationToken = default);

    Task ChangeMyPasswordAsync(Guid userId, ChangeStaffPasswordDto dto, CancellationToken cancellationToken = default);
}