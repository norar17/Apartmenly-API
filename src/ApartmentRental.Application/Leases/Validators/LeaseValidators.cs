using ApartmentRental.Application.Leases.DTOs;
using FluentValidation;

namespace ApartmentRental.Application.Leases.Validators;

public class CreateLeaseRequestValidator : AbstractValidator<CreateLeaseRequest>
{
    public CreateLeaseRequestValidator()
    {
        RuleFor(x => x.ApartmentId).NotEmpty();
        RuleFor(x => x.RenterId).NotEmpty();
        RuleFor(x => x.MonthlyRent).GreaterThan(0);
        RuleFor(x => x.DueDay).InclusiveBetween(1, 28);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public class RenewLeaseRequestValidator : AbstractValidator<RenewLeaseRequest>
{
    public RenewLeaseRequestValidator()
    {
        RuleFor(x => x.NewEndDate).GreaterThan(DateTime.UtcNow);
    }
}

public class TerminateLeaseRequestValidator : AbstractValidator<TerminateLeaseRequest>
{
    public TerminateLeaseRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
