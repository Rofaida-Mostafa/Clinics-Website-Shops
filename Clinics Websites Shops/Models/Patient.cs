namespace Clinics_Websites_Shops.Models
{
    public class Patient : IMustHaveTenant
    {
        public string PatientId { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
         public string TenantId { get; set; }
    }

}