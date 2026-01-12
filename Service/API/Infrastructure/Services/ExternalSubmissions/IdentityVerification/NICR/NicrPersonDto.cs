namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification.NICR;

public sealed class NicrPersonDto
{
    public string? IDNumber { get; set; }
    public string? Status { get; set; }       // "Active" | "Deceased" | "Person Not Found"
    public string? Surname { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }       // "M" | "F"
    public string? BirthDate { get; set; }    // "DD/MM/YYYY" (example shows "09/09/1999" with escaped slashes)
    public string? DeathDate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }
    public string? NationalityCode { get; set; }
    public string? IDCollected { get; set; }
    public string? BirthCertNumber { get; set; }
    public string? DistrictCode { get; set; }
    public string? District { get; set; }
    public string? CellPhoneNumber { get; set; }
    public string? Email { get; set; }
}