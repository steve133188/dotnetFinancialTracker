using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public class LoginFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a user to continue.")]
    public int SelectedUserId { get; set; }

    [Required(ErrorMessage = "PIN is required.")]
    [MinLength(4, ErrorMessage = "PIN must be at least 4 digits.")]
    public string Pin { get; set; } = string.Empty;
}
