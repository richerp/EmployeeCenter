using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class CreateServiceViewModel : UiStackLayoutViewModel
{
    [Required]
    [MaxLength(255)]
    [Display(Name = "Domain")]
    public string Domain { get; set; } = string.Empty;

    [Display(Name = "Owner")]
    public int? OwnerId { get; set; }

    [Display(Name = "Cross-Entity Link")]
    public int? CrossEntityLinkId { get; set; }

    [MaxLength(100)]
    [Display(Name = "Protocols")]
    public string? Protocols { get; set; }

    [Display(Name = "Location")]
    public int? LocationId { get; set; }

    [MaxLength(100)]
    [Display(Name = "Server IP")]
    public string? ServerIp { get; set; }

    [Display(Name = "Server")]
    public int? ServerId { get; set; }

    [Display(Name = "DNS Provider")]
    public int? DnsProviderId { get; set; }

    [Display(Name = "Via FRPS")]
    public bool IsViaFrps { get; set; }

    [Display(Name = "Cloudflare Proxied")]
    public bool IsCloudflareProxied { get; set; }

    [Display(Name = "Is Online")]
    public bool IsOnline { get; set; }

    [Display(Name = "Is Self Developed")]
    public bool IsSelfDeveloped { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    public List<CompanyEntity> AllOwners { get; set; } = new();
    public List<Location> AllLocations { get; set; } = new();
    public List<DnsProvider> AllDnsProviders { get; set; } = new();
    public List<Service> AllServices { get; set; } = new();
    public List<Server> AllServers { get; set; } = new();
}
