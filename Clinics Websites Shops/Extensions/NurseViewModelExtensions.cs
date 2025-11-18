using Clinics_Websites_Shops.Areas.Admin.ViewModel;
using Clinics_Websites_Shops.Models;
using Clinics_Websites_Shops.Services.IServices;

namespace Clinics_Websites_Shops.Extensions
{
    public static class NurseViewModelExtensions
    {
        public static Nurse ToEntity(this CreateNurseViewModel viewModel, string tenantId, string applicationUserId)
        {
            return new Nurse
            {
                NurseId = viewModel.NurseId,
                Salary = viewModel.Salary,
                ApplicationUserId = applicationUserId,
                TenantId = tenantId
            };
        }
        
        public static EditNurseViewModel ToEditViewModel(this Nurse nurse)
        {
            return new EditNurseViewModel
            {
                NurseId = nurse.NurseId,
                Salary = nurse.Salary,
                ApplicationUserId = nurse.ApplicationUserId,
                // Populate ApplicationUser fields if available
                Name = nurse.ApplicationUser?.Name ?? "",
                Email = nurse.ApplicationUser?.Email ?? "",
                PhoneNumber = nurse.ApplicationUser?.PhoneNumber ?? "",
                Description = nurse.ApplicationUser?.Description ?? "Nurse",
                Rate = nurse.ApplicationUser?.Rate ?? 5,
                // Password fields will be empty for security
                Password = "",
                ConfirmPassword = ""
            };
        }

        public static NurseListViewModel ToListViewModel(this Nurse nurse, ILocalizationService? localizationService = null)
        {
            return new NurseListViewModel
            {
                NurseId = nurse.NurseId,
                Salary = nurse.Salary,
                ApplicationUserName = nurse.ApplicationUser?.Name ?? "N/A",
                ApplicationUserEmail = nurse.ApplicationUser?.Email ?? "N/A",
                PhoneNumber = nurse.ApplicationUser?.PhoneNumber ?? "N/A",
                Rate = nurse.ApplicationUser?.Rate ?? 0,
                Description = nurse.ApplicationUser?.Description ?? "N/A",
                OriginalNurse = nurse
            };
        }
        
        public static void UpdateFromViewModel(this Nurse nurse, EditNurseViewModel viewModel)
        {
            nurse.NurseId = viewModel.NurseId;
            nurse.Salary = viewModel.Salary;
            nurse.ApplicationUserId = viewModel.ApplicationUserId;
        }
    }
}

