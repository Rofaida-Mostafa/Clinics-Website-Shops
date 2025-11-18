using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Models;

namespace Clinics_Websites_Shops.Extensions
{
    public static class AppointmentViewModelExtensions
    {
        /// <summary>
        /// Converts CreateAppointmentViewModel to Appointment entity
        /// </summary>
        public static Appointment ToEntity(this CreateAppointmentViewModel viewModel, string tenantId, string createdBy)
        {
            var appointment = new Appointment
            {
                AppointmentNumber = viewModel.AppointmentNumber,
                AppointmentDate = viewModel.AppointmentDate,
                AppointmentTime = viewModel.AppointmentTime,
                DurationMinutes = viewModel.DurationMinutes,
                AppointmentType = viewModel.AppointmentType,
                Priority = viewModel.Priority,
                ReasonForVisit = viewModel.ReasonForVisit,
                Notes = viewModel.Notes,
                PatientNotes = viewModel.PatientNotes,
                DoctorId = viewModel.DoctorId,
                PatientId = viewModel.PatientId,
                DepartmentId = viewModel.DepartmentId,
                ConsultationFee = viewModel.ConsultationFee,
                SendReminder = viewModel.SendReminder,
                SendSMS = viewModel.SendSMS,
                SendEmail = viewModel.SendEmail,
                Status = "Scheduled",
                IsConfirmed = false,
                IsCancelled = false,
                IsRescheduled = false,
                IsPaid = false,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            // Calculate end time
            var appointmentDateTime = appointment.AppointmentDate.Date + appointment.AppointmentTime;
            appointment.EndTime = appointmentDateTime.AddMinutes(appointment.DurationMinutes);

            return appointment;
        }

        /// <summary>
        /// Converts Appointment entity to EditAppointmentViewModel
        /// </summary>
        public static EditAppointmentViewModel ToEditViewModel(this Appointment appointment)
        {
            return new EditAppointmentViewModel
            {
                Id = appointment.Id,
                AppointmentNumber = appointment.AppointmentNumber,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                DurationMinutes = appointment.DurationMinutes,
                AppointmentType = appointment.AppointmentType,
                Priority = appointment.Priority,
                ReasonForVisit = appointment.ReasonForVisit,
                Notes = appointment.Notes,
                PatientNotes = appointment.PatientNotes,
                Status = appointment.Status,
                IsConfirmed = appointment.IsConfirmed,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                DepartmentId = appointment.DepartmentId,
                ConsultationFee = appointment.ConsultationFee,
                PaidAmount = appointment.PaidAmount,
                IsPaid = appointment.IsPaid,
                PaymentMethod = appointment.PaymentMethod,
                CheckInTime = appointment.CheckInTime,
                CheckOutTime = appointment.CheckOutTime,
                SendReminder = appointment.SendReminder,
                SendSMS = appointment.SendSMS,
                SendEmail = appointment.SendEmail
            };
        }

        /// <summary>
        /// Converts Appointment entity to AppointmentListViewModel
        /// </summary>
        public static AppointmentListViewModel ToListViewModel(this Appointment appointment)
        {
            return new AppointmentListViewModel
            {
                Id = appointment.Id,
                AppointmentNumber = appointment.AppointmentNumber,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                DurationMinutes = appointment.DurationMinutes,
                AppointmentType = appointment.AppointmentType,
                Priority = appointment.Priority,
                Status = appointment.Status,
                IsConfirmed = appointment.IsConfirmed,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor?.ApplicationUser?.Name ?? "N/A",
                DoctorSpecialization = appointment.Doctor?.Department != null
                    ? appointment.Doctor.Department.GetLocalizedName()
                    : "N/A",
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient?.ApplicationUser?.Name ?? "N/A",
                PatientPhone = appointment.Patient?.ApplicationUser?.PhoneNumber,
                DepartmentName = appointment.Department != null
                    ? appointment.Department.GetLocalizedName()
                    : "N/A",
                ConsultationFee = appointment.ConsultationFee,
                IsPaid = appointment.IsPaid,
                CreatedAt = appointment.CreatedAt
            };
        }

        /// <summary>
        /// Updates an existing Appointment entity from EditAppointmentViewModel
        /// </summary>
        public static void UpdateFromViewModel(this Appointment appointment, EditAppointmentViewModel viewModel, string modifiedBy)
        {
            appointment.AppointmentNumber = viewModel.AppointmentNumber;
            appointment.AppointmentDate = viewModel.AppointmentDate;
            appointment.AppointmentTime = viewModel.AppointmentTime;
            appointment.DurationMinutes = viewModel.DurationMinutes;
            appointment.AppointmentType = viewModel.AppointmentType;
            appointment.Priority = viewModel.Priority;
            appointment.ReasonForVisit = viewModel.ReasonForVisit;
            appointment.Notes = viewModel.Notes;
            appointment.PatientNotes = viewModel.PatientNotes;
            appointment.Status = viewModel.Status;
            appointment.IsConfirmed = viewModel.IsConfirmed;
            appointment.DoctorId = viewModel.DoctorId;
            appointment.PatientId = viewModel.PatientId;
            appointment.DepartmentId = viewModel.DepartmentId;
            appointment.ConsultationFee = viewModel.ConsultationFee;
            appointment.PaidAmount = viewModel.PaidAmount;
            appointment.IsPaid = viewModel.IsPaid;
            appointment.PaymentMethod = viewModel.PaymentMethod;
            appointment.CheckInTime = viewModel.CheckInTime;
            appointment.CheckOutTime = viewModel.CheckOutTime;
            appointment.SendReminder = viewModel.SendReminder;
            appointment.SendSMS = viewModel.SendSMS;
            appointment.SendEmail = viewModel.SendEmail;
            appointment.LastModified = DateTime.UtcNow;
            appointment.ModifiedBy = modifiedBy;

            // Recalculate end time
            var appointmentDateTime = appointment.AppointmentDate.Date + appointment.AppointmentTime;
            appointment.EndTime = appointmentDateTime.AddMinutes(appointment.DurationMinutes);

            // Update confirmation status
            if (viewModel.IsConfirmed && !appointment.IsConfirmed)
            {
                appointment.ConfirmedAt = DateTime.UtcNow;
                appointment.ConfirmedBy = modifiedBy;
            }
        }

        /// <summary>
        /// Helper method to get appointment status badge class
        /// </summary>
        public static string GetStatusBadgeClass(this AppointmentListViewModel appointment)
        {
            return appointment.Status?.ToLower() switch
            {
                "scheduled" => "bg-info",
                "confirmed" => "bg-primary",
                "inprogress" => "bg-warning",
                "completed" => "bg-success",
                "cancelled" => "bg-danger",
                "noshow" => "bg-secondary",
                "rescheduled" => "bg-info",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Helper method to get priority badge class
        /// </summary>
        public static string GetPriorityBadgeClass(this AppointmentListViewModel appointment)
        {
            return appointment.Priority?.ToLower() switch
            {
                "normal" => "bg-success",
                "urgent" => "bg-warning",
                "emergency" => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}

