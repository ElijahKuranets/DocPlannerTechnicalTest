namespace DocPlanner.Interfaces;

public interface ITimeSlot
{
    DateTime Start { get; set; }
    DateTime End { get; set; }
}