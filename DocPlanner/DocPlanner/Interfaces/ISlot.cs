namespace DocPlanner.Interfaces;

public interface ISlot
{
    DateTime Start { get; set; }
    DateTime End { get; set; }
}