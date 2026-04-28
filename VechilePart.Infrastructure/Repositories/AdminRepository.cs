using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VechilePart.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository {
    private readonly AppDbContext _context;

    public AdminRepository(AppDbContext context) {
        _context = context;
    }

    // Task 2: Save Staff User
    public async Task AddUserAsync(User user) { 
        _context.Users.Add(user); 
        await _context.SaveChangesAsync(); 
    }

    // Task 3: Save Part
    public async Task AddPartAsync(Part part) { 
        _context.Parts.Add(part); 
        await _context.SaveChangesAsync(); 
    }

    // Task 3: List Parts
    public async Task<List<Part>> GetPartsAsync() {
        return await _context.Parts.ToListAsync();
    }
}