using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel
{
	public class ConfirmOTPVM
	{
		public int Id { get; set; }
		[Required]
		public string OTPNumber { get; set; } = string.Empty;

		public string ApplicationUserId { get; set; }= string.Empty;
		
	}
}
