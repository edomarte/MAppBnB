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
    [Required]
    [Display(Name = "Birth Place")]
    public string? birth_place { get; set; }
    [Required]
    [Display(Name = "Birth Province")]
    public string? birth_province { get; set; }
    [Required]
    [Display(Name = "Birth Country")]
    public string? birth_country { get; set; }
    [DataType(DataType.Text)]
    [RegularExpression(@"^\+(?:[\d]{2,3})$")]
    [Display(Name = "Phone Prefix")]
    public string? phone_prefix { get; set; }

    //[RegularExpression(@"[\d]")]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone Number")]
    public string? phone_number { get; set; }
    [DataType(DataType.EmailAddress)]
    public string? email { get; set; }
    [Required]
    [Display(Name = "Role/Relation")]
    public RoleRelation? RoleRelation { get; set; }
    [Display(Name = "Document Serial Number")]
    public int?DocumentID{get;set;}

}