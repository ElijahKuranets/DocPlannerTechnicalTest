using DocPlanner.Interfaces;
using DocPlanner.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocPlanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly ISlotService _slotService;
    private readonly ILogger<AvailabilityController> _logger;

    public AvailabilityController(ISlotService slotService, ILogger<AvailabilityController> logger)
    {
        _slotService = slotService;
        _logger = logger;
    }
    
    [HttpGet("GetWeeklyAvailability/{date}")]
    public async Task<IActionResult> GetWeeklyAvailability(string date)
    {
        _logger.LogInformation("Getting weekly availability for date: {Date}", date);
        
        if (!DateTime.TryParseExact(
                date, 
                "yyyyMMdd", 
                null, 
                System.Globalization.DateTimeStyles.None, 
                out var parsedDate))
        {
            _logger.LogWarning("Invalid date format for GetWeeklyAvailability: {Date}", date);
            return BadRequest("Invalid date format. Please use yyyyMMdd format.");
        }

        try
        {
            var availability = await _slotService.GetWeeklyAvailabilityAsync(parsedDate);
            _logger.LogInformation("Weekly availability retrieved successfully for date: {Date}", date);
            return Ok(availability);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetWeeklyAvailability for date: {Date}", date);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
    
    [HttpGet("GetWeeklySlots/{date}")]
    public async Task<IActionResult> GetWeeklySlots(string date)
    {
        _logger.LogInformation("Getting weekly slots for date: {Date}", date);

        if (!DateTime.TryParseExact(
                date, 
                "yyyyMMdd", 
                null, 
                System.Globalization.DateTimeStyles.None, 
                out var parsedDate))
        {
            _logger.LogWarning("Invalid date format for GetWeeklySlots: {Date}", date);
            return BadRequest("Invalid date format. Please use yyyyMMdd format.");
        }

        try
        {
            var slots = await _slotService.GetWeeklySlotsAsync(parsedDate);
            if (slots == null || !slots.Any())
            {
                _logger.LogInformation("No slots available for date: {Date}", date);
                return NotFound("No slots available.");
            }

            _logger.LogInformation("Weekly slots retrieved successfully for date: {Date}", date);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetWeeklySlots for date: {Date}", date);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("TakeSlot")]
    public async Task<IActionResult> TakeSlot([FromBody] Slot slot)
    {
        _logger.LogInformation("Attempting to take a timeSlot");

        if (slot == null)
        {
            _logger.LogWarning("TimeSlot data is null in TakeSlot");
            return BadRequest("TimeSlot data is required.");
        }

        try
        {
            var success = await _slotService.TakeSlotAsync(slot);
            if (success)
            {
                _logger.LogInformation("TimeSlot successfully booked");
                return Ok("TimeSlot successfully booked.");
            }
            else
            {
                _logger.LogWarning("Failed to book the timeSlot");
                return BadRequest("Failed to book the timeSlot.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in TakeSlot");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}