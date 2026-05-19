using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehiclePart.Application;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Services;
using VehiclePart.Infrastructure.Data;
using VehiclePart.Infrastructure.Repositories;
using VehiclePart.Infrastructure.Services;

namespace VehiclePart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerHistoryRepository, CustomerHistoryRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();

        services.AddScoped<IJwtTokenFactory, JwtTokenFactory>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationJobRunner, NotificationJobRunner>();

        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ICustomerInviteService, CustomerInviteService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IVendorService, VendorService>();

        services.AddHostedService<NotificationBackgroundService>();

        return services;
    }
}
