using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PasswordsViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Passwords Management";
    }

    public List<Password> MyPasswords { get; set; } = [];
    public List<PasswordShare> SharedWithMe { get; set; } = [];
}
