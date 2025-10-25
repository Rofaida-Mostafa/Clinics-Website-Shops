namespace Clinics_Websites_Shops.Models
{
    public class Patient
    {
        public string PatientId { get; set; }
        public string? BloodType { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public string ApplicationUserId { get; set; }
        public Person? ApplicationUser { get; set; }

    }

}