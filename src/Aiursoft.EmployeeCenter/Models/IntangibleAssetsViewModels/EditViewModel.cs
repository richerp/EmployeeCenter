using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class EditViewModel : CreateViewModel
{
    [Required]
    public Guid Id { get; set; }
}
