using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    /// <summary>
    /// Represents a doctor's regular weekly schedule
    /// </summary>
    public class DoctorSchedule : IMustHaveTenant
    {
        public int Id { get; set; }

        // Doctor Reference
        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        // Day of Week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
        [Required]
        [Range(0, 6)]
        public int DayOfWeek { get; set; }

        // Working Hours
        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Break Time (optional)
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }

        // Slot Configuration
        [Range(5, 120)] // 5 minutes to 2 hours
        public int SlotDurationMinutes { get; set; } = 30; // Default 30-minute slots

        [Range(0, 60)]
        public int BufferTimeMinutes { get; set; } = 0; // Buffer time between appointments

        // Capacity
        [Range(1, 100)]
        public int MaxAppointmentsPerSlot { get; set; } = 1; // Usually 1, but can be more for group sessions

        // Location
        public int? LocationId { get; set; }
        public ClinicLocation? Location { get; set; }

        [StringLength(100)]
        public string? RoomNumber { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Effective Date Range (optional - for temporary schedule changes)
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModified { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;

        // Helper property to get day name
        [NotMapped]
        public string DayName => ((DayOfWeek)DayOfWeek).ToString();
    }
}

