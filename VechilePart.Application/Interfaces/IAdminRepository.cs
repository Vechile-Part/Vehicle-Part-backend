using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface IAdminRepository {
    
    Task AddUserAsync(User user);
    Task AddPartAsync(Part part);
    Task<List<Part>> GetPartsAsync();
}