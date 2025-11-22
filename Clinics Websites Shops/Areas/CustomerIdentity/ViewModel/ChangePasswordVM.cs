using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel
{
	public class ChangePasswordVM
	{
		[Required, DataType(DataType.Password)]
		public string CurrentPassword { get; set; } = string.Empty;
		[Required, DataType(DataType.Password)]
		public string NewPassword { get; set; } = string.Empty;
	}
}
