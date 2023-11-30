using DocPlanner.Interfaces;

namespace DocPlanner.Models;

public class Slot : ISlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}