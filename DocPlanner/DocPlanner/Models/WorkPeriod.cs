namespace DocPlanner.Models;

public class WorkPeriod
{
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int LunchStartHour { get; set; }
    public int LunchEndHour { get; set; }
    public List<Slot> BusySlots { get; set; }
    
}