namespace Clinics_Websites_Shops.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Status { get; set; }
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
