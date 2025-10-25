namespace Clinics_Websites_Shops.Models
{
    public class Nurse
    {
        public string NurseId { get; set; }
        public decimal Salary { get; set; }
        public string ApplicationUserId { get; set; }
        public Person? ApplicationUser { get; set; }

    }
}
