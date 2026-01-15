using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.CertificateViewModels;

public class PrintViewModel : UiStackLayoutViewModel
{
    public User? TargetUser { get; set; }
    public CertificateType Type { get; set; }
    public string Language { get; set; } = "zh-CN";
    public string CompanyName { get; set; } = string.Empty;
}