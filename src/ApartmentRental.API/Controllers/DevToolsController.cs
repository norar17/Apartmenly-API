using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.API.Controllers;

// Manual test endpoint for the email provider - trigger a real send on
// demand from Swagger or Postman instead of waiting for the daily reminder
// sweep. Reports the ACTUAL provider result (not just "request accepted")
// by reading back the log row EmailService just wrote. Owner-only, since
// anyone who can log in as Owner already has full access to the rest of
// the app. (SMS testing removed for now - see ISmsService if you bring it back.)
[Authorize(Policy = Policies.OwnerOnly)]
public class DevToolsController : ApiControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public DevToolsController(IEmailService emailService, IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("test-email")]
    public async Task<ActionResult<ApiResponse<TestSendResult>>> TestEmail(TestEmailRequest request, CancellationToken cancellationToken)
    {
        await _emailService.SendAsync(request.Email, request.Subject, $"<p>{request.Message}</p>", cancellationToken: cancellationToken);

        var log = await _unitOfWork.Repository<EmailLog>().Query()
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(l => l.ToAddress == request.Email, cancellationToken);

        return Ok(ApiResponse<TestSendResult>.Ok(new TestSendResult(
            log?.Status == CommunicationStatus.Sent,
            "Email",
            log?.ErrorMessage)));
    }
}

public record TestEmailRequest(string Email, string Subject, string Message);

public record TestSendResult(bool Sent, string Provider, string? ErrorMessage);
