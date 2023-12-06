namespace DocPlanner.Models;

public class Slot
{
    public Guid FacilityId { get; init; }
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public Patient? Patient { get; init; }
    public string? Comments { get; init; }
}

