using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface IVendorService
{
    Task<IReadOnlyList<VendorDto>> GetAllVendorsAsync(CancellationToken cancellationToken = default);
    Task<VendorDto> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VendorDto> CreateVendorAsync(CreateVendorDto dto, CancellationToken cancellationToken = default);
    Task<VendorDto> UpdateVendorAsync(Guid id, UpdateVendorDto dto, CancellationToken cancellationToken = default);
    Task DeleteVendorAsync(Guid id, CancellationToken cancellationToken = default);
}
