using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class Appointment : IMustHaveTenant
    {
        public int Id { get; set; }

        // Appointment Identity
        [Required, StringLength(50)]
        public string AppointmentNumber { get; set; } = null!; // Unique appointment number (e.g., "APT001")

        // Scheduling Information
        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Range(5, 480)] // 5 minutes to 8 hours
        public int DurationMinutes { get; set; } = 30; // Default 30 minutes

        public DateTime? EndTime { get; set; } // Calculated: AppointmentDate + AppointmentTime + Duration

        // Appointment Details
        [Required, StringLength(100)]
        public string AppointmentType { get; set; } = null!; // Consultation, Follow-up, Emergency, Surgery, etc.

        [StringLength(50)]
        public string? Priority { get; set; } // Normal, Urgent, Emergency

        [StringLength(1000)]
        public string? ReasonForVisit { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; } // Doctor's notes or special instructions

        [StringLength(2000)]
        public string? PatientNotes { get; set; } // Patient's notes or concerns

        // Status Management
        [Required, StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, InProgress, Completed, Cancelled, NoShow, Rescheduled

        public bool IsConfirmed { get; set; } = false;

        public DateTime? ConfirmedAt { get; set; }

        public string? ConfirmedBy { get; set; }

        // Cancellation Information
        public bool IsCancelled { get; set; } = false;

        public DateTime? CancelledAt { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        public string? CancelledBy { get; set; }

        // Rescheduling Information
        public bool IsRescheduled { get; set; } = false;

        public DateTime? RescheduledAt { get; set; }

        public int? PreviousAppointmentId { get; set; } // Link to previous appointment if rescheduled

        // Check-in/Check-out
        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        // Financial Information
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ConsultationFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaidAmount { get; set; }

        public bool IsPaid { get; set; } = false;

        [StringLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Card, Insurance, Online

        // Relationships
        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Payment? Payment { get; set; }

        // Reminder & Notification
        public bool SendReminder { get; set; } = true;

        public DateTime? ReminderSentAt { get; set; }

        public bool SendSMS { get; set; } = false;

        public bool SendEmail { get; set; } = true;

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

    // Enums for Appointment
    public enum AppointmentStatus
    {
        Scheduled,
        Confirmed,
        InProgress,
        Completed,
        Cancelled,
        NoShow,
        Rescheduled
    }

    public enum AppointmentPriority
    {
        Normal,
        Urgent,
        Emergency
    }
}
