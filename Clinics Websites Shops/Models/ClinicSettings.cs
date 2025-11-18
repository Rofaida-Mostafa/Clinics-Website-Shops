using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinics_Websites_Shops.Models
{
    public class ClinicSettings : IMustHaveTenant
    {
        public int Id { get; set; }

        // Clinic Information
        [Required, StringLength(200)]
        public string ClinicName { get; set; } = null!;

        [StringLength(500)]
        public string? ClinicDescription { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? Fax { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        // Business Hours
        [StringLength(500)]
        public string? BusinessHours { get; set; } // JSON format: {"Monday": "9:00-17:00", ...}

        [StringLength(100)]
        public string? TimeZone { get; set; }

        // Appointment Settings
        public int DefaultAppointmentDuration { get; set; } = 30; // minutes

        public int AppointmentSlotInterval { get; set; } = 15; // minutes

        public bool AllowOnlineBooking { get; set; } = true;

        public bool RequireAppointmentConfirmation { get; set; } = true;

        public int ReminderHoursBefore { get; set; } = 24;

        public bool SendSmsReminders { get; set; } = false;

        public bool SendEmailReminders { get; set; } = true;

        // Financial Settings
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(10)]
        public string? CurrencySymbol { get; set; } = "$";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DefaultConsultationFee { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; } // Percentage

        public bool RequirePaymentAtBooking { get; set; } = false;

        // Notification Settings
        public bool EnableEmailNotifications { get; set; } = true;

        public bool EnableSmsNotifications { get; set; } = false;

        [StringLength(200)]
        public string? SmtpServer { get; set; }

        public int? SmtpPort { get; set; }

        [StringLength(100)]
        public string? SmtpUsername { get; set; }

        [StringLength(500)]
        public string? SmtpPassword { get; set; } // Should be encrypted

        public bool SmtpUseSsl { get; set; } = true;

        [StringLength(100)]
        public string? SmsProvider { get; set; }

        [StringLength(500)]
        public string? SmsApiKey { get; set; } // Should be encrypted

        // Localization Settings
        [StringLength(10)]
        public string DefaultLanguage { get; set; } = "en";

        [StringLength(10)]
        public string DefaultCulture { get; set; } = "en-US";

        [StringLength(200)]
        public string? SupportedLanguages { get; set; } = "en,ar"; // Comma-separated

        // Security Settings
        public int SessionTimeoutMinutes { get; set; } = 60;

        public bool RequireTwoFactorAuth { get; set; } = false;

        public int PasswordExpiryDays { get; set; } = 90;

        public int MaxLoginAttempts { get; set; } = 5;

        // Feature Flags
        public bool EnablePatientPortal { get; set; } = true;

        public bool EnableOnlinePayments { get; set; } = false;

        public bool EnableTelehealth { get; set; } = false;

        public bool EnablePrescriptionManagement { get; set; } = true;

        public bool EnableLabResults { get; set; } = true;

        // Custom Settings (JSON)
        [Column(TypeName = "text")]
        public string? CustomSettings { get; set; } // JSON for additional custom settings

        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModified { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        [Required]
        public string TenantId { get; set; } = null!;
    }
}

