namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification.Golsabs;

public sealed class GolsabsCitizenDto
{
    public string? IDNumber { get; set; }
    public string? Status { get; set; }          // e.g. "Active"
    public string? Surname { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }          // e.g. "M"
    public string? BirthDate { get; set; }       // e.g. "10/03/2022"
    public string? DeathDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? CellPhoneNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? Email { get; set; }
}
