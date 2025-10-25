namespace Clinics_Websites_Shops.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Status { get; set; }
        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public Payment? Payment { get; set; }
    }
}
