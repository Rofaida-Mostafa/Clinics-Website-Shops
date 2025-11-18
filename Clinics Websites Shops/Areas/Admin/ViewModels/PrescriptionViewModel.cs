using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.Admin.ViewModels
{
    public class PrescriptionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Prescription number is required")]
        [Display(Name = "Prescription Number")]
        public string PrescriptionNumber { get; set; } = null!;

        [Required(ErrorMessage = "Prescription date is required")]
        [Display(Name = "Prescription Date")]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Medication name is required")]
        [Display(Name = "Medication Name")]
        [StringLength(200)]
        public string MedicationName { get; set; } = null!;

        [Display(Name = "Dosage")]
        [StringLength(100)]
        public string? Dosage { get; set; }

        [Display(Name = "Frequency")]
        [StringLength(100)]
        public string? Frequency { get; set; }

        [Display(Name = "Duration")]
        [StringLength(100)]
        public string? Duration { get; set; }

        [Display(Name = "Quantity")]
        public int? Quantity { get; set; }

        [Display(Name = "Route")]
        [StringLength(100)]
        public string? Route { get; set; }

        [Display(Name = "Instructions")]
        [DataType(DataType.MultilineText)]
        public string? Instructions { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

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

