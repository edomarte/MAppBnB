using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class Person
{
    public int id { get; set; }

    [Required]
    [DataType(DataType.Text)]
    public string? Name { get; set; }
    [Required]
    [DataType(DataType.Text)]
    public string? Surname { get; set; }
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Birth Date")]
    public DateOnly? BirthDate { get; set; }
    public int? Sex { get; set; }
    [Required]
    [Display(Name = "Birth Place")]
    public string? BirthPlace { get; set; }
    [Required]
    [Display(Name = "Birth Province")]
    public string? BirthProvince { get; set; }
    [Required]
    [Display(Name = "Birth Country")]
    public string? BirthCountry { get; set; }
    [DataType(DataType.Text)]
    [RegularExpression(@"^\+(?:[\d]{2,3})$")]
    [Display(Name = "Phone Prefix")]
    public string? PhonePrefix { get; set; }

    //[RegularExpression(@"[\d]")]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    [Required]
    [Display(Name = "Role/Relation")]
    public string? RoleRelation { get; set; }
    [Display(Name = "Document Serial Number")]
    public int? DocumentID { get; set; }
    [Required]
    public string? Citizenship{get;set;}

}