using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;
using VechilePart.Domain.Enums;

namespace VechilePart.Application.Services;

public class AdminService : IAdminService 
{
    private readonly IAdminRepository _repo;
    public AdminService(IAdminRepository repo) { _repo = repo; }

    public async Task RegisterStaffAsync(StaffRegistrationDto dto) {
        var user = new User { FullName = dto.FullName, Email = dto.Email, Role = RoleType.Staff };
        await _repo.AddUserAsync(user);
    }

    public async Task ManagePartAsync(Part part) { 
        await _repo.AddPartAsync(part);
    }

    public async Task<List<Part>> GetAllPartsAsync() => await _repo.GetPartsAsync();
}