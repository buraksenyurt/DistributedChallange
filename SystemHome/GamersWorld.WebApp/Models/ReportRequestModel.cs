using System.ComponentModel.DataAnnotations;

namespace GamersWorld.WebApp.Models;

public class ReportRequestModel
{
    [Required]
    public OwnerModel Owner { get; set; } = new OwnerModel
    {
        FullName = "Anonymous",
        Title = "Last Sales Report",
        EmployeeId = "CTO-66"
    };
    [Required]
    public string? ReportTitle { get; set; }
    [Required]
    public string? Expression { get; set; }
    public DateTime RequestTime { get; set; } = DateTime.Now;
}