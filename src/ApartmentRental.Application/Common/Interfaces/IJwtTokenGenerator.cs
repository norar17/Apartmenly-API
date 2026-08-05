using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, string fullName, UserRole role);
    string GenerateRefreshToken();
}
