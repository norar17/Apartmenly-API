using ApartmentRental.Application.Maintenance.DTOs;
using FluentValidation;

namespace ApartmentRental.Application.Maintenance.Validators;

public class CreateMaintenanceRequestRequestValidator : AbstractValidator<CreateMaintenanceRequestRequest>
{
    public CreateMaintenanceRequestRequestValidator()
    {
        RuleFor(x => x.ApartmentId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
