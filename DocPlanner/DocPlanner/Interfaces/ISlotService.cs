using DocPlanner.Models;

namespace DocPlanner.Interfaces;

public interface ISlotService
{
    Task<AvailabilityResponse> GetWeeklyAvailabilityAsync(DateTime date);
    Task<IEnumerable<Slot>> GetWeeklySlotsAsync(DateTime date);
    Task<bool> TakeSlotAsync(Slot slot);
}