using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel
{
    public class RegisterVM
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

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

    }
}
