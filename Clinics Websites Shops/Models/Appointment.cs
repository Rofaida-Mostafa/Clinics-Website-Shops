namespace Clinics_Websites_Shops.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Status { get; set; }
        public string? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public string? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public Payment? Payment { get; set; }
    }
}
