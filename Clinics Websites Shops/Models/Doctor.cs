namespace Clinics_Websites_Shops.Models
{
    public class Doctor
    {
        public string DoctorId { get; set; }
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public decimal Salary { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public string ApplicationUserId { get; set; }
        public Person? ApplicationUser { get; set; }

    }
}
