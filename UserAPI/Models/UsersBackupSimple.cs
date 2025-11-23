using System;
using System.Collections.Generic;

namespace UserAPI.Models;

public partial class UsersBackupSimple
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Country { get; set; }

    public string? Address { get; set; }

    public string? ProfileImagePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool? IsTermsAccepted { get; set; }

    public string State { get; set; } = null!;

    public string City { get; set; } = null!;

    public int? RoleId { get; set; }

    public int? CountryId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }
}
