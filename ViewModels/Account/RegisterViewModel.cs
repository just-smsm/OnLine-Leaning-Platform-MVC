using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.ViewModels.Account;

public class RegisterViewModel {
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Url]
    [Display(Name = "Profile Picture URL")]
    public byte[]? ProfilePictureUrl { get; set; }
}