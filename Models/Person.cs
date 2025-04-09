using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class Person
{
    public int id { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(30, ErrorMessage = "Name cannot exceed 30 characters")]
    public string? Name { get; set; }
    [Required]
    [MaxLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
    [DataType(DataType.Text)]
    public string? Surname { get; set; }
    [DataType(DataType.Date)]
    [Display(Name = "Birth Date")]
    public DateOnly? BirthDate { get; set; }
    public int? Sex { get; set; }
    [Display(Name = "Birth Place")]
    public string? BirthPlace { get; set; }
    [Display(Name = "Birth Province")]
    public string? BirthProvince { get; set; }
    [Display(Name = "Birth Country")]
    public string? BirthCountry { get; set; }
    [DataType(DataType.Text)]
    [RegularExpression(@"^\+(?:[\d]{2,3})$", ErrorMessage = "Prefix must start with + and 2-3 numbers")]
    [Display(Name = "Phone Prefix")]
    public string? PhonePrefix { get; set; }
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    [Required]
    [Display(Name = "Role/Relation")]
    public int? RoleRelation { get; set; }
    [Display(Name = "Document ID")]
    public int? DocumentID { get; set; }
    [Required]
    public string? Citizenship { get; set; }

}