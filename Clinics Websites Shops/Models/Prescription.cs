using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class Prescription : IMustHaveTenant
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string PrescriptionNumber { get; set; } = null!;

        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(200)]
        public string MedicationName { get; set; } = null!;

        [StringLength(100)]
        public string? Dosage { get; set; }

        [StringLength(100)]
        public string? Frequency { get; set; } // e.g., "Twice daily", "Every 8 hours"

        [StringLength(100)]
        public string? Duration { get; set; } // e.g., "7 days", "2 weeks"

        public int? Quantity { get; set; }

        [StringLength(100)]
        public string? Route { get; set; } // e.g., "Oral", "Topical", "Injection"

        [Column(TypeName = "text")]
        public string? Instructions { get; set; }

        [Column(TypeName = "text")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Relationships
        public int? ReportId { get; set; }
        public Report? Report { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

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
