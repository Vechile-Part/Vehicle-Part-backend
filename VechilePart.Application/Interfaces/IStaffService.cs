using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface IStaffService
{
    Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default);
}
