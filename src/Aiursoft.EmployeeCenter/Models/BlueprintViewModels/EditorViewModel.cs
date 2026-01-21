using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.BlueprintViewModels;

public class EditorViewModel : UiStackLayoutViewModel
{
    public EditorViewModel()
    {
        PageTitle = "Edit Blueprint";
    }
    public int? DocumentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string InputMarkdown { get; set; } = string.Empty;

    public string OutputHtml { get; set; } = string.Empty;

    public bool SavedSuccessfully { get; set; }

    public bool IsEditing => DocumentId.HasValue;

    public string? PublicLink { get; set; }
}
