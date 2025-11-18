using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class ClinicLocation : IMustHaveTenant
    {
        public int Id { get; set; }

        // Location Identity
        [Required, StringLength(100)]
        [Display(Name = "Location Name")]
        public string LocationName { get; set; } = null!;

        [StringLength(50)]
        [Display(Name = "Location Code")]
        public string? LocationCode { get; set; } // e.g., "LOC001", "MAIN", "BRANCH1"

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Address Information
        [Required, StringLength(500)]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "Area/District")]
        public string? Area { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; } = null!;

        // Contact Information
        [Required, StringLength(20)]
        [Display(Name = "Primary Phone")]
        public string PrimaryPhone { get; set; } = null!;

        [StringLength(20)]
        [Display(Name = "Secondary Phone")]
        public string? SecondaryPhone { get; set; }

        [StringLength(20)]
        [Display(Name = "Fax")]
        public string? Fax { get; set; }

        [Required, EmailAddress, StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "Alternative Email")]
        public string? AlternativeEmail { get; set; }

        [StringLength(200)]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        // Map Coordinates
        [Column(TypeName = "decimal(10,7)")]
        [Display(Name = "Latitude")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        [Display(Name = "Longitude")]
        public decimal? Longitude { get; set; }

        [StringLength(1000)]
        [Display(Name = "Map Embed URL")]
        public string? MapEmbedUrl { get; set; } // Google Maps embed URL

        [StringLength(500)]
        [Display(Name = "Directions URL")]
        public string? DirectionsUrl { get; set; } // Google Maps directions link

        // Business Hours (JSON format)
        [Column(TypeName = "text")]
        [Display(Name = "Business Hours")]
        public string? BusinessHours { get; set; } // JSON: {"Monday": {"open": "09:00", "close": "17:00", "closed": false}, ...}

        [StringLength(100)]
        [Display(Name = "Time Zone")]
        public string? TimeZone { get; set; }

        // Location Settings
        [Display(Name = "Is Default Location")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Accepts Appointments")]
        public bool AcceptsAppointments { get; set; } = true;

        [Display(Name = "Accepts Walk-ins")]
        public bool AcceptsWalkIns { get; set; } = true;

        [Display(Name = "Has Emergency Services")]
        public bool HasEmergencyServices { get; set; } = false;

        [Display(Name = "Has Pharmacy")]
        public bool HasPharmacy { get; set; } = false;

        [Display(Name = "Has Laboratory")]
        public bool HasLaboratory { get; set; } = false;

        [Display(Name = "Has Radiology")]
        public bool HasRadiology { get; set; } = false;

        // Capacity
        [Display(Name = "Number of Rooms")]
        public int? NumberOfRooms { get; set; }

        [Display(Name = "Number of Beds")]
        public int? NumberOfBeds { get; set; }

        [Display(Name = "Parking Capacity")]
        public int? ParkingCapacity { get; set; }

        // Additional Information
        [StringLength(1000)]
        [Display(Name = "Facilities")]
        public string? Facilities { get; set; } // Comma-separated: "WiFi, Parking, Wheelchair Access, ..."

        [StringLength(1000)]
        [Display(Name = "Services Offered")]
        public string? ServicesOffered { get; set; } // Comma-separated

        [Column(TypeName = "text")]
        [Display(Name = "Special Notes")]
        public string? SpecialNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModified { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;
    }
}

