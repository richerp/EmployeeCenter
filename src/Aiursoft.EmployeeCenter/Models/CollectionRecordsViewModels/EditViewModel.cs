using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.CollectionRecordsViewModels;

public class EditViewModel : CreateViewModel
{
    [Required]
    public int Id { get; set; }
}
