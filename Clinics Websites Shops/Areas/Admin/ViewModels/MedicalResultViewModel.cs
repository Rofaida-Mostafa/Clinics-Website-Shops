using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModels
{
    public class MedicalResultViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Result number is required")]
        [Display(Name = "Result Number")]
        public string ResultNumber { get; set; } = null!;

        [Required(ErrorMessage = "Test name is required")]
        [Display(Name = "Test Name")]
        [StringLength(200)]
        public string TestName { get; set; } = null!;

        [Display(Name = "Test Category")]
        [StringLength(100)]
        public string? TestCategory { get; set; }

        [Display(Name = "Test Code")]
        [StringLength(100)]
        public string? TestCode { get; set; }

        [Required(ErrorMessage = "Test date is required")]
        [Display(Name = "Test Date")]
        [DataType(DataType.Date)]
        public DateTime TestDate { get; set; } = DateTime.Now;

        [Display(Name = "Result Date")]
        [DataType(DataType.Date)]
        public DateTime? ResultDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Result Value")]
        [DataType(DataType.MultilineText)]
        public string? ResultValue { get; set; }

        [Display(Name = "Findings")]
        [DataType(DataType.MultilineText)]
        public string? Findings { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Abnormal")]
        public bool IsAbnormal { get; set; } = false;

        [Display(Name = "Critical")]
        public bool IsCritical { get; set; } = false;

        [Display(Name = "Attachment")]
        public IFormFile? AttachmentFile { get; set; }

        [Display(Name = "Current Attachment")]
        public string? AttachmentUrl { get; set; }

        public string? AttachmentFileName { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Display(Name = "Doctor")]
        public int? DoctorId { get; set; }

        [Display(Name = "Appointment")]
        public int? AppointmentId { get; set; }

        [Display(Name = "Report")]
        public int? ReportId { get; set; }

        // For display purposes
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public string? AppointmentNumber { get; set; }
    }
}

