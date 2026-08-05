using ApartmentRental.Application.Apartments.DTOs;
using FluentValidation;

namespace ApartmentRental.Application.Apartments.Validators;

public class CreateApartmentRequestValidator : AbstractValidator<CreateApartmentRequest>
{
    public CreateApartmentRequestValidator()
    {
        RuleFor(x => x.ApartmentNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlyRent).GreaterThan(0);
        RuleFor(x => x.Deposit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bathrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public class UpdateApartmentRequestValidator : AbstractValidator<UpdateApartmentRequest>
{
    public UpdateApartmentRequestValidator()
    {
        RuleFor(x => x.ApartmentNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlyRent).GreaterThan(0);
        RuleFor(x => x.Deposit).GreaterThanOrEqualTo(0);
    }
}
