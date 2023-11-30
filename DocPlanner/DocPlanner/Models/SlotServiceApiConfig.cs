using DocPlannerTechnicalTest.Interfaces;

namespace DocPlanner.Models;

public class SlotServiceApiConfig : ISlotServiceApiConfig
{
    public string BaseUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}