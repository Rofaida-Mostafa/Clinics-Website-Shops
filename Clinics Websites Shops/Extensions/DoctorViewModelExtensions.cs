using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clinics_Websites_Shops.Extensions
{
    public static class DoctorViewModelExtensions
    {
        public static Doctor ToEntity(this CreateDoctorViewModel viewModel, string tenantId, string applicationUserId)
        {
            return new Doctor
            {
                DoctorId = viewModel.DoctorId,
                LicenseNumber = viewModel.LicenseNumber,
                ApplicationUserId = applicationUserId,
                DepartmentId = viewModel.DepartmentId,
                MedicalDegree = viewModel.MedicalDegree,
                AdditionalCertifications = viewModel.AdditionalCertifications,
                YearsOfExperience = viewModel.YearsOfExperience,
                MedicalSchool = viewModel.MedicalSchool,
                GraduationDate = viewModel.GraduationDate,
                Rank = viewModel.Rank,
                EmploymentStatus = viewModel.EmploymentStatus,
                JoinedDate = viewModel.JoinedDate,
                BaseSalary = viewModel.BaseSalary,
                ConsultationFee = viewModel.ConsultationFee,
                HourlyRate = viewModel.HourlyRate,
                WorkingHoursStart = viewModel.WorkingHoursStart,
                WorkingHoursEnd = viewModel.WorkingHoursEnd,
                WorkingDays = viewModel.WorkingDays,
                Biography = viewModel.Biography,
                Languages = viewModel.Languages,
                Awards = viewModel.Awards,
                DirectPhone = viewModel.DirectPhone,
                OfficeLocation = viewModel.OfficeLocation,
                RoomNumber = viewModel.RoomNumber,
                EmergencyContactName = viewModel.EmergencyContactName,
                EmergencyContactPhone = viewModel.EmergencyContactPhone,
                EmergencyContactRelation = viewModel.EmergencyContactRelation,
                ProfileImageUrl = viewModel.ProfileImageUrl,
                IsAvailableForEmergency = viewModel.IsAvailableForEmergency,
                IsAcceptingNewPatients = viewModel.IsAcceptingNewPatients,
                IsActive = viewModel.IsActive,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        public static EditDoctorViewModel ToEditViewModel(this Doctor doctor)
        {
            return new EditDoctorViewModel
            {
                Id = doctor.Id,
                DoctorId = doctor.DoctorId,
                LicenseNumber = doctor.LicenseNumber,
                ApplicationUserId = doctor.ApplicationUserId,
                DepartmentId = doctor.DepartmentId,
                // Populate ApplicationUser fields if available
                Name = doctor.ApplicationUser?.Name ?? "",
                Email = doctor.ApplicationUser?.Email ?? "",
                PhoneNumber = doctor.ApplicationUser?.PhoneNumber ?? "",
                Description = doctor.ApplicationUser?.Description ?? "Doctor",
                Rate = doctor.ApplicationUser?.Rate ?? 5,
                MedicalDegree = doctor.MedicalDegree,
                AdditionalCertifications = doctor.AdditionalCertifications,
                YearsOfExperience = doctor.YearsOfExperience,
                MedicalSchool = doctor.MedicalSchool,
                GraduationDate = doctor.GraduationDate,
                Rank = doctor.Rank,
                EmploymentStatus = doctor.EmploymentStatus,
                JoinedDate = doctor.JoinedDate,
                BaseSalary = doctor.BaseSalary,
                ConsultationFee = doctor.ConsultationFee,
                HourlyRate = doctor.HourlyRate,
                WorkingHoursStart = doctor.WorkingHoursStart,
                WorkingHoursEnd = doctor.WorkingHoursEnd,
                WorkingDays = doctor.WorkingDays,
                Biography = doctor.Biography,
                Languages = doctor.Languages,
                Awards = doctor.Awards,
                DirectPhone = doctor.DirectPhone,
                OfficeLocation = doctor.OfficeLocation,
                RoomNumber = doctor.RoomNumber,
                EmergencyContactName = doctor.EmergencyContactName,
                EmergencyContactPhone = doctor.EmergencyContactPhone,
                EmergencyContactRelation = doctor.EmergencyContactRelation,
                ProfileImageUrl = doctor.ProfileImageUrl,
                IsAvailableForEmergency = doctor.IsAvailableForEmergency,
                IsAcceptingNewPatients = doctor.IsAcceptingNewPatients,
                IsActive = doctor.IsActive
            };
        }
        
        public static DoctorListViewModel ToListViewModel(this Doctor doctor, ILocalizationService? localizationService = null)
        {
            return new DoctorListViewModel
            {
                Id = doctor.Id,
                DoctorId = doctor.DoctorId,
                LicenseNumber = doctor.LicenseNumber,
                MedicalDegree = doctor.MedicalDegree,
                YearsOfExperience = doctor.YearsOfExperience,
                Rank = doctor.Rank,
                EmploymentStatus = doctor.EmploymentStatus,
                BaseSalary = doctor.BaseSalary,
                IsActive = doctor.IsActive,
                DepartmentName = doctor.Department?.GetLocalizedName(localizationService?.GetCurrentCulture() ?? "en") ?? "N/A",
                ApplicationUserName = doctor.ApplicationUser?.UserName ?? "N/A",
                ApplicationUserEmail = doctor.ApplicationUser?.Email ?? "N/A",
                ProfileImageUrl = doctor.ProfileImageUrl,
                JoinedDate = doctor.JoinedDate,
                OriginalDoctor = doctor
            };
        }
        
        public static void UpdateFromViewModel(this Doctor doctor, EditDoctorViewModel viewModel)
        {
            doctor.DoctorId = viewModel.DoctorId;
            doctor.LicenseNumber = viewModel.LicenseNumber;
            doctor.ApplicationUserId = viewModel.ApplicationUserId;
            doctor.DepartmentId = viewModel.DepartmentId;
            doctor.MedicalDegree = viewModel.MedicalDegree;
            doctor.AdditionalCertifications = viewModel.AdditionalCertifications;
            doctor.YearsOfExperience = viewModel.YearsOfExperience;
            doctor.MedicalSchool = viewModel.MedicalSchool;
            doctor.GraduationDate = viewModel.GraduationDate;
            doctor.Rank = viewModel.Rank;
            doctor.EmploymentStatus = viewModel.EmploymentStatus;
            doctor.JoinedDate = viewModel.JoinedDate;
            doctor.BaseSalary = viewModel.BaseSalary;
            doctor.ConsultationFee = viewModel.ConsultationFee;
            doctor.HourlyRate = viewModel.HourlyRate;
            doctor.WorkingHoursStart = viewModel.WorkingHoursStart;
            doctor.WorkingHoursEnd = viewModel.WorkingHoursEnd;
            doctor.WorkingDays = viewModel.WorkingDays;
            doctor.Biography = viewModel.Biography;
            doctor.Languages = viewModel.Languages;
            doctor.Awards = viewModel.Awards;
            doctor.DirectPhone = viewModel.DirectPhone;
            doctor.OfficeLocation = viewModel.OfficeLocation;
            doctor.RoomNumber = viewModel.RoomNumber;
            doctor.EmergencyContactName = viewModel.EmergencyContactName;
            doctor.EmergencyContactPhone = viewModel.EmergencyContactPhone;
            doctor.EmergencyContactRelation = viewModel.EmergencyContactRelation;
            doctor.ProfileImageUrl = viewModel.ProfileImageUrl;
            doctor.IsAvailableForEmergency = viewModel.IsAvailableForEmergency;
            doctor.IsAcceptingNewPatients = viewModel.IsAcceptingNewPatients;
            doctor.IsActive = viewModel.IsActive;
            doctor.LastModified = DateTime.UtcNow;
        }
        
        public static List<SelectListItem> ToSelectList(this IEnumerable<Doctor> doctors)
        {
            return doctors.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.DoctorId} - {d.ApplicationUser?.UserName ?? "Unknown"}"
            }).ToList();
        }
    }
}