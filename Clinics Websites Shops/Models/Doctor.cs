namespace Clinics_Websites_Shops.Models
{
    public class Doctor: Person
    {
        public int DoctorId { get; set; }
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public decimal Salary { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
