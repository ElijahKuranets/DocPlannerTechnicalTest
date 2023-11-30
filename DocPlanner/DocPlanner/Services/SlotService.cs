using System.Text;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using Newtonsoft.Json;

namespace DocPlanner.Services;

public class SlotService : ISlotService
{
    private readonly HttpClient _httpClientFactory;
    private readonly ILogger<SlotService> _logger;

    public SlotService(IHttpClientFactory httpClientFactory, ILogger<SlotService> logger)
    {
        _httpClientFactory = httpClientFactory.CreateClient("Base64AuthClient");
        _logger = logger;
    }

    public async Task<AvailabilityResponse> GetWeeklyAvailabilityAsync(DateTime date)
    {
        var formattedDate = date.ToString("yyyyMMdd");
        _logger.LogInformation("Requesting weekly availability for date: {Date}", formattedDate);

        try
        {
            var response = await _httpClientFactory.GetAsync($"GetWeeklyAvailability/{formattedDate}");

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
    
    public async Task<IEnumerable<Slot>> GetWeeklySlotsAsync(DateTime date)
    {
        var formattedDate = date.ToString("yyyyMMdd");
        _logger.LogInformation("Requesting weekly slots for date: {Date}", formattedDate);
        
        try
        {
            var response = await _httpClientFactory.GetAsync($"GetWeeklySlots/{formattedDate}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var availability = JsonConvert.DeserializeObject<IEnumerable<Slot>>(content);
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
        _logger.LogInformation("Requesting taking slot ");
        try
        {
            var jsonContent = JsonConvert.SerializeObject(slot);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClientFactory.PostAsync("TakeSlot", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while taking slot");
            throw; 
        }
    }
}