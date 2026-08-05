using ApartmentRental.Application.Payments.DTOs;
using FluentValidation;

namespace ApartmentRental.Application.Payments.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.LeaseId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.DueDate).NotEmpty();
    }
}

public class MarkPaymentPaidRequestValidator : AbstractValidator<MarkPaymentPaidRequest>
{
    public MarkPaymentPaidRequestValidator()
    {
        RuleFor(x => x.PaymentDate).NotEmpty();
    }
}
