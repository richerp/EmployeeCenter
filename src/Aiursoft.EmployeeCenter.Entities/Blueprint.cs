using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class Blueprint
{
    [Key]
    public int Id { get; init; }

    [MaxLength(200)]
    public required string Title { get; set; }

    public required string Content { get; set; } // Markdown

    public required string RenderedHtml { get; set; }

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    public required string AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }
}
