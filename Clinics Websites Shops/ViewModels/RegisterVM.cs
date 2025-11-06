namespace Clinics_Websites_Shops.ViewModels
{
    public class RegisterVM
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public string DomainName { get; set; } = null!; // example: nameclinic.com
    }
}
