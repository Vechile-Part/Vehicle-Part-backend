using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface IStaffRepository
{
    Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
}
