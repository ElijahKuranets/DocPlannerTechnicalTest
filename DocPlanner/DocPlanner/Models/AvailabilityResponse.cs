namespace DocPlanner.Models;

public class AvailabilityResponse
{
    public Facility Facility { get; set; }
    public int SlotDurationMinutes { get; set; }
    
    public WeekDay Monday { get; set; }
    public WeekDay Tuesday { get; set; }
    public WeekDay Wednesday { get; set; }
    public WeekDay Thursday { get; set; }
    public WeekDay Friday { get; set; }
    public WeekDay Saturday { get; set; }
    public WeekDay Sunday { get; set; }
}