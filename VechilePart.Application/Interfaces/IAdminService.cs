using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface IAdminService {
    Task RegisterStaffAsync(StaffRegistrationDto dto);
    Task ManagePartAsync(Part part); 
    Task<List<Part>> GetAllPartsAsync(); 
}