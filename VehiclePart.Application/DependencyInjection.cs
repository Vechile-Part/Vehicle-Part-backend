using Microsoft.Extensions.DependencyInjection;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Services;

namespace VehiclePart.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerAuthService, CustomerAuthService>();
        services.AddScoped<ICustomerHistoryService, CustomerHistoryService>();
        services.AddScoped<IEmailInvoiceService, EmailInvoiceService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        return services;
    }
}
