using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class MedicalResult : IMustHaveTenant
    {
        public int Id { get; set; }

        // Result Identity
        [Required, StringLength(50)]
        public string ResultNumber { get; set; } = null!; // Unique result number (e.g., "RES001")

        // Test Information
        [Required, StringLength(200)]
        public string TestName { get; set; } = null!; // e.g., "Complete Blood Count", "X-Ray Chest"

        [StringLength(100)]
        public string? TestCategory { get; set; } // e.g., "Laboratory", "Radiology", "Pathology"

        [StringLength(100)]
        public string? TestCode { get; set; } // Internal test code

        // Result Details
        [Required]
        public DateTime TestDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResultDate { get; set; } // When results were finalized

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Reviewed

        [Column(TypeName = "text")]
        public string? ResultValue { get; set; } // Actual test results (can be JSON for complex results)

        [Column(TypeName = "text")]
        public string? Findings { get; set; } // Doctor's findings/interpretation

        [Column(TypeName = "text")]
        public string? Notes { get; set; } // Additional notes

        [StringLength(50)]
        public string? ResultStatus { get; set; } // Normal, Abnormal, Critical

        public bool IsAbnormal { get; set; } = false;

        public bool IsCritical { get; set; } = false;

        // File Attachments
        [StringLength(500)]
        public string? AttachmentUrl { get; set; } // URL to uploaded file (PDF, image, etc.)

        [StringLength(100)]
        public string? AttachmentFileName { get; set; }

        [StringLength(50)]
        public string? AttachmentFileType { get; set; } // PDF, JPG, PNG, DICOM, etc.

        // Lab/Technician Information
        [StringLength(200)]
        public string? PerformedBy { get; set; } // Lab technician or radiologist name

        [StringLength(200)]
        public string? ReviewedBy { get; set; } // Doctor who reviewed the results

        public DateTime? ReviewedAt { get; set; }

        [StringLength(200)]
        public string? Laboratory { get; set; } // External lab name if applicable

        // Reference Values
        [StringLength(100)]
        public string? ReferenceRange { get; set; } // Normal range for the test

        [StringLength(50)]
        public string? Unit { get; set; } // Unit of measurement (mg/dL, mmol/L, etc.)

        // Relationships
        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? DoctorId { get; set; } // Ordering doctor
        public Doctor? Doctor { get; set; }

        public int? AppointmentId { get; set; } // Related appointment
        public Appointment? Appointment { get; set; }

        public int? ReportId { get; set; } // Related medical report
        public Report? Report { get; set; }

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

    public enum ResultStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Reviewed = 4,
        Cancelled = 5
    }

    public enum ResultType
    {
        Normal = 1,
        Abnormal = 2,
        Critical = 3
    }
}

