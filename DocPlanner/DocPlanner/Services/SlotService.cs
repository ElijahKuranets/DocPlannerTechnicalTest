using System.Text;
using DocPlanner.Clients;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using Newtonsoft.Json;

namespace DocPlanner.Services;

public class SlotService : ISlotService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlotService> _logger;

    public SlotService(DocPlannerClient docPlanner, ILogger<SlotService> logger)
    {
        _httpClient = docPlanner.Client;
        _logger = logger;
    }

    public async Task<AvailabilityResponse> GetWeeklyAvailabilityAsync(DateTime date)
    {
        var formattedDate = date.ToString("yyyyMMdd");
        _logger.LogInformation("Requesting weekly availability for date: {Date}", formattedDate);

        try
        {
            var response = await _httpClient.GetAsync($"GetWeeklyAvailability/{formattedDate}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var availability = JsonConvert.DeserializeObject<AvailabilityResponse>(content);
                return availability;
            }
            else
            {
                _logger.LogWarning("Failed to get weekly availability for date: {Date}. Status Code: {StatusCode}", formattedDate, response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting weekly availability for date: {Date}", formattedDate);
            throw; 
        }
    }
    
    public async Task<IEnumerable<TimeSlot>> GetWeeklySlotsAsync(DateTime date)
    {
        var formattedDate = date.ToString("yyyyMMdd");
        _logger.LogInformation("Requesting weekly slots for date: {Date}", formattedDate);
        
        try
        {
            var response = await _httpClient.GetAsync($"GetWeeklySlots/{formattedDate}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var availability = JsonConvert.DeserializeObject<IEnumerable<TimeSlot>>(content);
                return availability;
            }
            else
            {
                _logger.LogWarning("Failed to get weekly slots for date: {Date}. Status Code: {StatusCode}", formattedDate, response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting weekly slots for date: {Date}", formattedDate);
            throw; 
        }
    }

    public async Task<bool> TakeSlotAsync(Slot slot)
    {
        _logger.LogInformation("Requesting taking Slot ");
        try
        {
            var start = slot.Start.ToString("yyyy-MM-dd HH:mm:ss");
            var end = slot.End.ToString("yyyy-MM-dd HH:mm:ss");
            var updatedSlot = new
            {
                FacilityId = slot.FacilityId,
                Start = start,
                End = end,
                Patient = slot.Patient,
                Comments = slot.Comments
            }; 
            var jsonContent = JsonConvert.SerializeObject(updatedSlot);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("TakeSlot", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while taking Slot");
            throw; 
        }
    }
}