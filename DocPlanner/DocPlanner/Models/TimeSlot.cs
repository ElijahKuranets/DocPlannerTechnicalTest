using DocPlanner.Interfaces;

namespace DocPlanner.Models;

public class TimeSlot : ITimeSlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}