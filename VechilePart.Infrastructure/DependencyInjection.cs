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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDataStore>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ICustomerFeatureService, CustomerFeatureService>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        return services;
    }
}