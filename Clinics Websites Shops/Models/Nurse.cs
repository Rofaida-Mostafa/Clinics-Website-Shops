namespace Clinics_Websites_Shops.Models
{
    public class Nurse:Person
    {
        public int NurseId { get; set; }
        public decimal Salary { get; set; }
        public string UserId { get; set; }
        public Person? ApplicationUser { get; set; }

    }
}
