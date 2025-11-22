using System.ComponentModel.DataAnnotations;

namespace Clinics_Websites_Shops.Areas.CustomerIdentity.ViewModel
{
	public class LoginVM
	{
		public int Id { get; set; }
		[Required]
		public string EmailORUserName { get; set; }=string.Empty;
		[Required,DataType(DataType.Password)]
		public string Password { get; set; }= string.Empty;

		public bool RememberMe {get; set;}
	}
}
