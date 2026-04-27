using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VechilePart.Application.Interfaces;
using VechilePart.Application.Services;
using VechilePart.Infrastructure.Data;
using VechilePart.Infrastructure.Repositories;

namespace VechilePart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAdminRepository, AdminRepository>();
        return services;
    }
}
