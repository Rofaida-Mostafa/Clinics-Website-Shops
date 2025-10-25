namespace Clinics_Websites_Shops.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
