namespace Clinics_Websites_Shops.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        
        //department localzation
    }
}
