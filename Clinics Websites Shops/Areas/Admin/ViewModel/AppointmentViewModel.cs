using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModel
{
    // ViewModel for creating a new appointment
    public class CreateAppointmentViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Appointment number is required")]
        [Display(Name = "Appointment Number")]
        [StringLength(50)]
        public string AppointmentNumber { get; set; } = null!;

        [Required(ErrorMessage = "Appointment date is required")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Appointment time is required")]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Display(Name = "Duration (Minutes)")]
        [Range(5, 480, ErrorMessage = "Duration must be between 5 and 480 minutes")]
        public int DurationMinutes { get; set; } = 30;

        [Required(ErrorMessage = "Appointment type is required")]
        [Display(Name = "Appointment Type")]
        [StringLength(100)]
        public string AppointmentType { get; set; } = null!;

        [Display(Name = "Priority")]
        [StringLength(50)]
        public string? Priority { get; set; } = "Normal";

        [Display(Name = "Reason for Visit")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string? ReasonForVisit { get; set; }

        [Display(Name = "Notes")]
        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Patient Notes")]
        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string? PatientNotes { get; set; }

        [Required(ErrorMessage = "Doctor is required")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Consultation Fee")]
        [Range(0, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal? ConsultationFee { get; set; }

        [Display(Name = "Send Reminder")]
        public bool SendReminder { get; set; } = true;

        [Display(Name = "Send SMS")]
        public bool SendSMS { get; set; } = false;

        [Display(Name = "Send Email")]
        public bool SendEmail { get; set; } = true;

        // Dropdown lists (populated by controller)
        public List<SelectListItem>? Doctors { get; set; }
        public List<SelectListItem>? Patients { get; set; }
        public List<SelectListItem>? Departments { get; set; }
        public List<SelectListItem>? AppointmentTypes { get; set; }
        public List<SelectListItem>? Priorities { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate appointment date is not in the past
            if (AppointmentDate.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Appointment date cannot be in the past",
                    new[] { nameof(AppointmentDate) });
            }

            // Validate appointment time is not in the past for today's appointments
            if (AppointmentDate.Date == DateTime.Today)
            {
                var appointmentDateTime = AppointmentDate.Date + AppointmentTime;
                if (appointmentDateTime < DateTime.Now)
                {
                    yield return new ValidationResult(
                        "Appointment time cannot be in the past",
                        new[] { nameof(AppointmentTime) });
                }
            }
        }
    }

    // ViewModel for editing an existing appointment
    public class EditAppointmentViewModel : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Appointment number is required")]
        [Display(Name = "Appointment Number")]
        [StringLength(50)]
        public string AppointmentNumber { get; set; } = null!;

        [Required(ErrorMessage = "Appointment date is required")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment time is required")]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Display(Name = "Duration (Minutes)")]
        [Range(5, 480, ErrorMessage = "Duration must be between 5 and 480 minutes")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Appointment type is required")]
        [Display(Name = "Appointment Type")]
        [StringLength(100)]
        public string AppointmentType { get; set; } = null!;

        [Display(Name = "Priority")]
        [StringLength(50)]
        public string? Priority { get; set; }

        [Display(Name = "Reason for Visit")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string? ReasonForVisit { get; set; }

        [Display(Name = "Notes")]
        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Patient Notes")]
        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string? PatientNotes { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        [Display(Name = "Is Confirmed")]
        public bool IsConfirmed { get; set; }

        [Required(ErrorMessage = "Doctor is required")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Consultation Fee")]
        [Range(0, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal? ConsultationFee { get; set; }

        [Display(Name = "Paid Amount")]
        [Range(0, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal? PaidAmount { get; set; }

        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; }

        [Display(Name = "Payment Method")]
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Check-in Time")]
        [DataType(DataType.DateTime)]
        public DateTime? CheckInTime { get; set; }

        [Display(Name = "Check-out Time")]
        [DataType(DataType.DateTime)]
        public DateTime? CheckOutTime { get; set; }

        [Display(Name = "Send Reminder")]
        public bool SendReminder { get; set; }

        [Display(Name = "Send SMS")]
        public bool SendSMS { get; set; }

        [Display(Name = "Send Email")]
        public bool SendEmail { get; set; }

        // Dropdown lists (populated by controller)
        public List<SelectListItem>? Doctors { get; set; }
        public List<SelectListItem>? Patients { get; set; }
        public List<SelectListItem>? Departments { get; set; }
        public List<SelectListItem>? AppointmentTypes { get; set; }
        public List<SelectListItem>? Priorities { get; set; }
        public List<SelectListItem>? Statuses { get; set; }
        public List<SelectListItem>? PaymentMethods { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate paid amount doesn't exceed consultation fee
            if (PaidAmount.HasValue && ConsultationFee.HasValue && PaidAmount > ConsultationFee)
            {
                yield return new ValidationResult(
                    "Paid amount cannot exceed consultation fee",
                    new[] { nameof(PaidAmount) });
            }

            // Validate check-out time is after check-in time
            if (CheckInTime.HasValue && CheckOutTime.HasValue && CheckOutTime < CheckInTime)
            {
                yield return new ValidationResult(
                    "Check-out time must be after check-in time",
                    new[] { nameof(CheckOutTime) });
            }
        }
    }

    // ViewModel for displaying appointment in list/grid
    public class AppointmentListViewModel
    {
        public int Id { get; set; }
        public string AppointmentNumber { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public int DurationMinutes { get; set; }
        public string AppointmentType { get; set; } = null!;
        public string? Priority { get; set; }
        public string Status { get; set; } = null!;
        public bool IsConfirmed { get; set; }
        
        // Doctor Information
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? DoctorSpecialization { get; set; }

        // Patient Information
        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public string? PatientPhone { get; set; }
        
        // Department Information
        public string? DepartmentName { get; set; }
        
        // Financial
        public decimal? ConsultationFee { get; set; }
        public bool IsPaid { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}

