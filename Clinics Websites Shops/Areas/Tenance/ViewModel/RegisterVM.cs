using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.Tenance.ViewModel
{
    public class RegisterVM
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic name is required")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Clinic name must contain letters only")]
        public string ClinicName { get; set; } = null!; // example: nameclinic

        [Required]
        public List<string> AvailableLocales { get; set; } = new List<string>
    {
        "en",
        "ar",
        "fr"
    };
    }
}
