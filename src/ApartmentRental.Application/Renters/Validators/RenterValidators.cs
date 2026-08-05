using ApartmentRental.Application.Renters.DTOs;
using ApartmentRental.Shared.Constants;
using FluentValidation;

namespace ApartmentRental.Application.Renters.Validators;

public class CreateRenterRequestValidator : AbstractValidator<CreateRenterRequest>
{
    public CreateRenterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(RegexPatterns.PhMobileNumber).WithMessage(RegexPatterns.PhMobileNumberMessage);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class UpdateRenterRequestValidator : AbstractValidator<UpdateRenterRequest>
{
    public UpdateRenterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(RegexPatterns.PhMobileNumber).WithMessage(RegexPatterns.PhMobileNumberMessage);
    }
}
