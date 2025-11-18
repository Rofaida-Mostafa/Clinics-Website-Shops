using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Models
{
    /// <summary>
    /// Represents a doctor's holiday, time off, or unavailable period
    /// </summary>
    public class DoctorHoliday : IMustHaveTenant
    {
        public int Id { get; set; }

        // Doctor Reference
        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        // Holiday Details
        [Required, StringLength(200)]
        public string Title { get; set; } = null!; // e.g., "Annual Leave", "Conference", "Sick Leave"

        [StringLength(1000)]
        public string? Description { get; set; }

        // Date Range
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Time Range (optional - for partial day off)
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        // Holiday Type
        [Required, StringLength(50)]
        public string HolidayType { get; set; } = "PersonalLeave"; // PersonalLeave, SickLeave, Conference, Training, PublicHoliday, Emergency, Other

        // Full Day or Partial
        public bool IsFullDay { get; set; } = true;

        // Recurring Holiday (e.g., every Friday)
        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurrencePattern { get; set; } // Weekly, Monthly, Yearly

        public int? RecurrenceDayOfWeek { get; set; } // 0-6 for weekly recurrence

        // Status
        [Required, StringLength(50)]
        public string Status { get; set; } = "Approved"; // Pending, Approved, Rejected, Cancelled

        public bool IsApproved { get; set; } = true;

        public DateTime? ApprovedAt { get; set; }

        [StringLength(450)]
        public string? ApprovedBy { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Appointment Handling
        public bool BlockNewAppointments { get; set; } = true; // Prevent new appointments during this period

        public bool CancelExistingAppointments { get; set; } = false; // Auto-cancel existing appointments

        [StringLength(1000)]
        public string? NotificationMessage { get; set; } // Message to send to patients with appointments

        // Replacement Doctor (optional)
        public int? ReplacementDoctorId { get; set; }
        public Doctor? ReplacementDoctor { get; set; }

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

    // Enums for DoctorHoliday
    public enum HolidayType
    {
        PersonalLeave,
        SickLeave,
        Conference,
        Training,
        PublicHoliday,
        Emergency,
        Other
    }

    public enum HolidayStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}

