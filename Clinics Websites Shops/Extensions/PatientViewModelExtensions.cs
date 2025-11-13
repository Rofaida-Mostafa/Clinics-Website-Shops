using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Models;

namespace Clinics_Websites_Shops.Extensions
{
    public static class PatientViewModelExtensions
    {
        public static Patient ToEntity(this CreatePatientViewModel viewModel, string tenantId, string applicationUserId)
        {
            return new Patient
            {
                PatientId = viewModel.PatientId,
                ApplicationUserId = applicationUserId,
                NationalId = viewModel.NationalId,
                PassportNumber = viewModel.PassportNumber,
                DateOfBirth = viewModel.DateOfBirth,
                Age = viewModel.Age,
                Gender = viewModel.Gender,
                Nationality = viewModel.Nationality,
                Occupation = viewModel.Occupation,
                MaritalStatus = viewModel.MaritalStatus,
                Address = viewModel.Address,
                City = viewModel.City,
                State = viewModel.State,
                PostalCode = viewModel.PostalCode,
                Country = viewModel.Country,
                AlternatePhone = viewModel.AlternatePhone,
                EmergencyContactName = viewModel.EmergencyContactName,
                EmergencyContactPhone = viewModel.EmergencyContactPhone,
                EmergencyContactRelation = viewModel.EmergencyContactRelation,
                EmergencyContactAddress = viewModel.EmergencyContactAddress,
                BloodType = viewModel.BloodType,
                Height = viewModel.Height,
                Weight = viewModel.Weight,
                Allergies = viewModel.Allergies,
                ChronicDiseases = viewModel.ChronicDiseases,
                CurrentMedications = viewModel.CurrentMedications,
                PreviousSurgeries = viewModel.PreviousSurgeries,
                FamilyMedicalHistory = viewModel.FamilyMedicalHistory,
                IsSmoker = viewModel.IsSmoker,
                IsAlcoholConsumer = viewModel.IsAlcoholConsumer,
                SpecialNotes = viewModel.SpecialNotes,
                InsuranceProvider = viewModel.InsuranceProvider,
                InsurancePolicyNumber = viewModel.InsurancePolicyNumber,
                InsuranceExpiryDate = viewModel.InsuranceExpiryDate,
                InsuranceCoverageAmount = viewModel.InsuranceCoverageAmount,
                RegistrationDate = viewModel.RegistrationDate,
                Status = viewModel.Status,
                IsActive = viewModel.IsActive,
                ProfileImageUrl = viewModel.ProfileImageUrl,
                PreferredLanguage = viewModel.PreferredLanguage,
                PreferredContactMethod = viewModel.PreferredContactMethod,
                AllowSmsNotifications = viewModel.AllowSmsNotifications,
                AllowEmailNotifications = viewModel.AllowEmailNotifications,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        public static EditPatientViewModel ToEditViewModel(this Patient patient)
        {
            return new EditPatientViewModel
            {
                Id = patient.Id,
                PatientId = patient.PatientId,
                ApplicationUserId = patient.ApplicationUserId,
                // Populate ApplicationUser fields if available
                Name = patient.ApplicationUser?.Name ?? "",
                Email = patient.ApplicationUser?.Email ?? "",
                PhoneNumber = patient.ApplicationUser?.PhoneNumber ?? "",
                Description = patient.ApplicationUser?.Description ?? "Patient",
                Rate = patient.ApplicationUser?.Rate ?? 5,
                NationalId = patient.NationalId,
                PassportNumber = patient.PassportNumber,
                DateOfBirth = patient.DateOfBirth,
                Age = patient.Age,
                Gender = patient.Gender,
                Nationality = patient.Nationality,
                Occupation = patient.Occupation,
                MaritalStatus = patient.MaritalStatus,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                PostalCode = patient.PostalCode,
                Country = patient.Country,
                AlternatePhone = patient.AlternatePhone,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                EmergencyContactRelation = patient.EmergencyContactRelation,
                EmergencyContactAddress = patient.EmergencyContactAddress,
                BloodType = patient.BloodType,
                Height = patient.Height,
                Weight = patient.Weight,
                Allergies = patient.Allergies,
                ChronicDiseases = patient.ChronicDiseases,
                CurrentMedications = patient.CurrentMedications,
                PreviousSurgeries = patient.PreviousSurgeries,
                FamilyMedicalHistory = patient.FamilyMedicalHistory,
                IsSmoker = patient.IsSmoker,
                IsAlcoholConsumer = patient.IsAlcoholConsumer,
                SpecialNotes = patient.SpecialNotes,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                InsuranceExpiryDate = patient.InsuranceExpiryDate,
                InsuranceCoverageAmount = patient.InsuranceCoverageAmount,
                RegistrationDate = patient.RegistrationDate,
                Status = patient.Status,
                IsActive = patient.IsActive,
                ProfileImageUrl = patient.ProfileImageUrl,
                PreferredLanguage = patient.PreferredLanguage,
                PreferredContactMethod = patient.PreferredContactMethod,
                AllowSmsNotifications = patient.AllowSmsNotifications,
                AllowEmailNotifications = patient.AllowEmailNotifications
            };
        }
        
        public static PatientListViewModel ToListViewModel(this Patient patient)
        {
            return new PatientListViewModel
            {
                Id = patient.Id,
                PatientId = patient.PatientId,
                NationalId = patient.NationalId,
                DateOfBirth = patient.DateOfBirth,
                Age = patient.Age,
                Gender = patient.Gender,
                BloodType = patient.BloodType,
                RegistrationDate = patient.RegistrationDate,
                LastVisitDate = patient.LastVisitDate,
                Status = patient.Status,
                IsActive = patient.IsActive,
                ApplicationUserName = patient.ApplicationUser?.Name ?? "N/A",
                ApplicationUserEmail = patient.ApplicationUser?.Email ?? "N/A",
                ApplicationUserPhone = patient.ApplicationUser?.PhoneNumber ?? "N/A",
                ProfileImageUrl = patient.ProfileImageUrl,
                InsuranceProvider = patient.InsuranceProvider,
                OriginalPatient = patient
            };
        }
        
        public static void UpdateFromViewModel(this Patient patient, EditPatientViewModel viewModel)
        {
            patient.PatientId = viewModel.PatientId;
            patient.NationalId = viewModel.NationalId;
            patient.PassportNumber = viewModel.PassportNumber;
            patient.DateOfBirth = viewModel.DateOfBirth;
            patient.Age = viewModel.Age;
            patient.Gender = viewModel.Gender;
            patient.Nationality = viewModel.Nationality;
            patient.Occupation = viewModel.Occupation;
            patient.MaritalStatus = viewModel.MaritalStatus;
            patient.Address = viewModel.Address;
            patient.City = viewModel.City;
            patient.State = viewModel.State;
            patient.PostalCode = viewModel.PostalCode;
            patient.Country = viewModel.Country;
            patient.AlternatePhone = viewModel.AlternatePhone;
            patient.EmergencyContactName = viewModel.EmergencyContactName;
            patient.EmergencyContactPhone = viewModel.EmergencyContactPhone;
            patient.EmergencyContactRelation = viewModel.EmergencyContactRelation;
            patient.EmergencyContactAddress = viewModel.EmergencyContactAddress;
            patient.BloodType = viewModel.BloodType;
            patient.Height = viewModel.Height;
            patient.Weight = viewModel.Weight;
            patient.Allergies = viewModel.Allergies;
            patient.ChronicDiseases = viewModel.ChronicDiseases;
            patient.CurrentMedications = viewModel.CurrentMedications;
            patient.PreviousSurgeries = viewModel.PreviousSurgeries;
            patient.FamilyMedicalHistory = viewModel.FamilyMedicalHistory;
            patient.IsSmoker = viewModel.IsSmoker;
            patient.IsAlcoholConsumer = viewModel.IsAlcoholConsumer;
            patient.SpecialNotes = viewModel.SpecialNotes;
            patient.InsuranceProvider = viewModel.InsuranceProvider;
            patient.InsurancePolicyNumber = viewModel.InsurancePolicyNumber;
            patient.InsuranceExpiryDate = viewModel.InsuranceExpiryDate;
            patient.InsuranceCoverageAmount = viewModel.InsuranceCoverageAmount;
            patient.RegistrationDate = viewModel.RegistrationDate;
            patient.Status = viewModel.Status;
            patient.IsActive = viewModel.IsActive;
            patient.ProfileImageUrl = viewModel.ProfileImageUrl;
            patient.PreferredLanguage = viewModel.PreferredLanguage;
            patient.PreferredContactMethod = viewModel.PreferredContactMethod;
            patient.AllowSmsNotifications = viewModel.AllowSmsNotifications;
            patient.AllowEmailNotifications = viewModel.AllowEmailNotifications;
            patient.LastModified = DateTime.UtcNow;
        }
    }
}

