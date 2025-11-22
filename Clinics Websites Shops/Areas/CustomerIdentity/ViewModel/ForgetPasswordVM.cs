using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel
{
	public class ForgetPasswordVM
	{
		public int Id { get; set; }
		[Required]
		public string EmailORUserName { get; set; } = string.Empty;
	}
}
