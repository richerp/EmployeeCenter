using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;

namespace Aiursoft.EmployeeCenter.Models.CollectionRecordsViewModels;

public class EditViewModel : CreateViewModel
{
    [Required]
    public int Id { get; set; }
}
