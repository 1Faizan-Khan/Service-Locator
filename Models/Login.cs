using System.ComponentModel.DataAnnotations;
namespace ServiceLocator.Models;

public class Login
{
     [Required(ErrorMessage = "This field is required")]
    public String Email { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public String Password { get; set; }






}