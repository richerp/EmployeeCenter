using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.EmployeeCenter.Models.PasswordsViewModels;

public class ManageSharesViewModel : UiStackLayoutViewModel
{
    public ManageSharesViewModel()
    {
        PageTitle = "Manage Password Shares";
    }

    public Guid PasswordId { get; set; }
    public string? PasswordTitle { get; set; }
    public List<PasswordShare> ExistingShares { get; set; } = [];
    public List<IdentityRole> AvailableRoles { get; set; } = [];
}
