namespace Clinics_Websites_Shops.Models
{
    public class Nurse : IMustHaveTenant
    {
        public string NurseId { get; set; }
        public decimal Salary { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public string TenantId { get; set; }

    }
}
