using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class VendorService(IVendorRepository repository) : IVendorService
{
    public async Task<IReadOnlyList<VendorDto>> GetAllVendorsAsync(CancellationToken cancellationToken = default)
    {
        var vendors = await repository.GetAllVendorsAsync(cancellationToken);
        return vendors.Select(MapToDto).ToList();
    }

    public async Task<VendorDto> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await repository.GetVendorByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor with ID '{id}' not found.");
        return MapToDto(vendor);
    }

    public async Task<VendorDto> CreateVendorAsync(CreateVendorDto dto, CancellationToken cancellationToken = default)
    {
        var vendor = new Vendor
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            Address = dto.Address.Trim(),
            CompanyRegistrationNumber = NormalizeOptional(dto.CompanyRegistrationNumber),
            IsActive = true
        };

        var created = await repository.AddVendorAsync(vendor, cancellationToken);
        return MapToDto(created);
    }

    public async Task<VendorDto> UpdateVendorAsync(Guid id, UpdateVendorDto dto, CancellationToken cancellationToken = default)
    {
        var vendor = await repository.GetVendorByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor with ID '{id}' not found.");

        vendor.Name = dto.Name.Trim();
        vendor.ContactPerson = dto.ContactPerson.Trim();
        vendor.Phone = dto.Phone.Trim();
        vendor.Email = dto.Email.Trim();
        vendor.Address = dto.Address.Trim();
        vendor.CompanyRegistrationNumber = NormalizeOptional(dto.CompanyRegistrationNumber);
        vendor.IsActive = dto.IsActive;

        await repository.UpdateVendorAsync(vendor, cancellationToken);
        return MapToDto(vendor);
    }

    public async Task DeleteVendorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ = await repository.GetVendorByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor with ID '{id}' not found.");

        await repository.DeleteVendorAsync(id, cancellationToken);
    }

    private static VendorDto MapToDto(Vendor v) =>
        new(
            v.Id,
            v.Name,
            v.ContactPerson,
            v.Email,
            v.Phone,
            v.Address,
            v.CompanyRegistrationNumber,
            v.IsActive);

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return value.Trim();
    }
}
