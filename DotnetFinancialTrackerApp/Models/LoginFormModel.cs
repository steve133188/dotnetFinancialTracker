using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public class LoginFormModel
{
    [Required(ErrorMessage = "Select a user to continue.")]
    public string SelectedUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "PIN is required.")]
    [MinLength(4, ErrorMessage = "PIN must be at least 4 digits.")]
    public string Pin { get; set; } = string.Empty;
}
