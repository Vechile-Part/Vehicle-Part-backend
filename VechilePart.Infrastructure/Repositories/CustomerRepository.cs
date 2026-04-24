using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        return Task.FromResult(customer);
    }

    public Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Add(vehicle);
        return Task.FromResult(vehicle);
    }

    public Task<Appointment> AddAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Add(appointment);
        return Task.FromResult(appointment);
    }

    public Task<PartRequest> AddPartRequestAsync(PartRequest request, CancellationToken cancellationToken = default)
    {
        dbContext.PartRequests.Add(request);
        return Task.FromResult(request);
    }

    public Task<ServiceReview> AddServiceReviewAsync(ServiceReview review, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceReviews.Add(review);
        return Task.FromResult(review);
    }

    public Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dbContext.Customers.FirstOrDefault(x => x.Id == customerId));
    }

    public Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<Vehicle>)dbContext.Vehicles);
    public Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<SalesInvoice>)dbContext.SalesInvoices);
}
