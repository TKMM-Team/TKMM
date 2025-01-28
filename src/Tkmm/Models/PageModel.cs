using FluentAvalonia.UI.Controls;

namespace Tkmm.Models;

public class PageModel
{
    public required string Title { get; set; }
    public object? Content { get; set; }
    public required Symbol Icon { get; set; }
    public string? Description { get; set; }
}
