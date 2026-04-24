using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Services;

public class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public async Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            GovernmentId = dto.GovernmentId
        }, cancellationToken);

        return customer.Id;
    }

    public Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddVehicleAsync(new Vehicle
        {
            CustomerId = customerId,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, cancellationToken);
    }

    public Task BookAppointmentAsync(AppointmentDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddAppointmentAsync(new Appointment
        {
            CustomerId = dto.CustomerId,
            AppointmentAtUtc = dto.AppointmentAtUtc,
            Notes = dto.Notes
        }, cancellationToken);
    }

    public Task RequestPartAsync(PartRequestDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddPartRequestAsync(new PartRequest
        {
            CustomerId = dto.CustomerId,
            PartName = dto.PartName,
            Notes = dto.Notes
        }, cancellationToken);
    }

    public Task AddServiceReviewAsync(ServiceReviewDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddServiceReviewAsync(new ServiceReview
        {
            CustomerId = dto.CustomerId,
            Rating = dto.Rating,
            Comment = dto.Comment
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<object>> GetPurchaseAndServiceHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var invoices = (await repository.GetSalesInvoicesAsync(cancellationToken)).Where(x => x.CustomerId == customerId).Select(x => (object)x);
        return invoices.ToList();
    }
}
