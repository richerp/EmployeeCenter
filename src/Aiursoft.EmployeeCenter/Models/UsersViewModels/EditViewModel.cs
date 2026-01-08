using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Aiursoft.CSTools.Attributes;
using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.EmployeeCenter.Models.UsersViewModels;

// Manage if a role is selected or not in the UI.
public class UserRoleViewModel
{
    public required string RoleName { get; set; }
    public bool IsSelected { get; set; }
}

public class EditViewModel : UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit User";
        AllRoles = [];
    }

    [Required(ErrorMessage = "The {0} is required.")]
    [Display(Name = "User name")]
    [ValidDomainName]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "The {0} is required.")]
    [Display(Name = "Name")]
    public required string DisplayName { get; set; }

    [Required(ErrorMessage = "The {0} is required.")]
    [EmailAddress(ErrorMessage = "The {0} is not a valid email address.")]
    [Display(Name = "Email Address")]
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Reset Password (leave empty to keep the same password)")]
    public string? Password { get; set; }

    [NotNull]
    [Display(Name = "Avatar file")]
    [Required(ErrorMessage = "The avatar file is required.")]
    [RegularExpression(@"^Workspace/avatar.*", ErrorMessage = "The avatar file is invalid. Please upload it again.")]
    [MaxLength(150)]
    [MinLength(2)]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Creation Time")]
    public DateTime CreationTime { get; set; }

    [Display(Name = "Job Level")]
    [MaxLength(100)]
    public string? JobLevel { get; set; }

    [Display(Name = "Title")]
    [MaxLength(100)]
    public string? Title { get; set; }

    [Display(Name = "Legal Name")]
    [MaxLength(100)]
    public string? LegalName { get; set; }

    [Display(Name = "Phone Number")]
    [Phone]
    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Base Salary")]
    public decimal BaseSalary { get; set; }

    [Display(Name = "Bank Account (Card Number)")]
    [MaxLength(30)]
    public string? BankAccount { get; set; }

    [Display(Name = "Bank Name (Branch)")]
    [MaxLength(30)]
    public string? BankName { get; set; }

    [Display(Name = "Bank Account Name (Holder)")]
    [MaxLength(30)]
    public string? BankAccountName { get; set; }

    [Display(Name = "Manager")]
    public string? ManagerId { get; set; }

    public string? ManagerDisplayName { get; set; }

    [Required(ErrorMessage = "The {0} is required.")]
    [FromRoute]
    public required string Id { get; set; }

    public List<UserRoleViewModel> AllRoles { get; set; }
}
