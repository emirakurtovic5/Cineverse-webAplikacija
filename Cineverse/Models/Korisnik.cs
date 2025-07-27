using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Cineverse.Models
{
    public class Korisnik : IdentityUser
    {
        [DisplayName("Ime:")]
        public string? Ime { get; set; }

        [DisplayName("Prezime:")]
        public string? Prezime { get; set; }

        [DisplayName("Datum rođenja:")]
        public DateTime? DatumRodjenja { get; set; }

        public override string? PhoneNumber { get; set; }

        
    }
}

public class ValidateDate : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; 
        }

        if (!(value is DateTime datumRodjenja))
        {
            return new ValidationResult("Neispravan datum rođenja.");
        }

        DateTime danas = DateTime.Today;
        int godine = danas.Year - datumRodjenja.Year;

        
        if (datumRodjenja.Date > danas.AddYears(-godine))
        {
            godine--;
        }

        if (godine < 12 || godine > 100)
        {
            return new ValidationResult("Dozvoljeno je samo za korisnike između 12 i 100 godina.");
        }

        return ValidationResult.Success;
    }
}