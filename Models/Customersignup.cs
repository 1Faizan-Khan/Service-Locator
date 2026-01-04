using System.ComponentModel.DataAnnotations;
namespace ServiceLocator.Models;
using System.ComponentModel.DataAnnotations.Schema;
public class Customersignup
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }


    [Required(ErrorMessage = "This field is required")]
    [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Invalid input: Only letters and spaces are allowed")]
    public String Name { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; }

    [Required(ErrorMessage = "State is required")]
    public string State { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public String Zipcode { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public String Phone { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public String Email { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public String Password { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public String Whatservice { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [Range(1, 100, ErrorMessage = "Radius must be between 1 and 100")]
    public int Radius { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public String Description { get; set; }





}