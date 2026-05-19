namespace VehiclePart.Application.Interfaces;

public interface IJwtTokenFactory
{
    string CreateToken(IReadOnlyList<(string ClaimType, string Value)> claims);
}
