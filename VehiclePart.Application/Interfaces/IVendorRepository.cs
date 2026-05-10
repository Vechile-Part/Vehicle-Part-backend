using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface IVendorRepository
{
    Task<IReadOnlyList<Vendor>> GetAllVendorsAsync(CancellationToken cancellationToken = default);
    Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vendor> AddVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task DeleteVendorAsync(Guid id, CancellationToken cancellationToken = default);
}
