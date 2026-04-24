using Microsoft.Extensions.DependencyInjection;
using VechilePart.Application.Interfaces;
using VechilePart.Application.Services;
using VechilePart.Infrastructure.Data;
using VechilePart.Infrastructure.Repositories;

namespace VechilePart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddSingleton<AppDbContext>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        return services;
    }
}
