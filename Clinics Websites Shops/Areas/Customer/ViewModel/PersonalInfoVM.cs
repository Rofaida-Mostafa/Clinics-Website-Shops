namespace  Clinics_Websites_Shops.Areas.Customer.ViewModels;

public class PersonalInfoVM
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    
    //list of skills
    public List<string> Skills { get; set; } = new List<string>();
}