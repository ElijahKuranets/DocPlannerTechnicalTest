using Asp.Versioning;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocPlanner.Controllers;

[Authorize]
[ApiVersion("1.0")]
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

    /// <summary>
    /// Retrieves the weekly availability and busy slots for a doctor at a specific facility for a given date.
    /// </summary>
    /// <remarks>
    /// This endpoint returns the weekly availability including busy slots for each day of the week based on the provided date. 
    /// It is designed to help patients or administrators understand when appointments can be scheduled.
    /// 
    /// Sample request:
    ///
    ///     GET /GetWeeklyAvailability/20231120
    ///
    /// The date must be in the 'yyyyMMdd' format. The response includes availability details for the entire week of the given date.
    ///
    /// </remarks>
    /// <param name="date">The date in format 'yyyyMMdd' to check availability for. The date should represent the week for which the availability is needed.</param>
    /// <returns>An object containing the availability information including FacilityId, doctor's details, hospital address, slot duration, and busy slots for each weekday of the provided date's week.</returns>
    /// <response code="200">Returns the weekly availability details if the date is valid and data is available.</response>
    /// <response code="400">If the date is null, not provided, or in an incorrect format.</response>
    /// <response code="500">If an internal server error occurs while processing the request.</response> 
    [HttpGet("GetWeeklyAvailability/{date}")]
    public async Task<IActionResult> GetWeeklyAvailability(string date)
    {
        _logger.LogInformation("Getting weekly availability for date: {Date}", date);
        
        if (!IsValidDateFormat(date, out var parsedDate))
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

    /// <summary>
    /// Gets the weekly slots for a specified date.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /GetWeeklySlots/20231120
    ///
    /// </remarks>
    /// <param name="date">The date in format yyyyMMdd to check availability for.</param>
    /// <returns>A list of available slots for the week of the provided date.</returns>
    /// <response code="200">Returns the available slots</response>
    /// <response code="400">If the date is null or in an incorrect format</response>
    /// <response code="500">If an internal server error occurs while processing the request</response> 
    [HttpGet("GetWeeklySlots/{date}")]
    public async Task<IActionResult> GetWeeklySlots(string date)
    {
        _logger.LogInformation("Getting weekly slots for date: {Date}", date);

        if (!IsValidDateFormat(date, out var parsedDate))
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

    /// <summary>
    /// Attempts to book a time slot for a doctor's appointment.
    /// </summary>
    /// <remarks>
    /// This method takes a Slot object containing the necessary information to book a specific time slot, such as the start and end times, the facility ID, and patient details.
    ///
    /// Sample request:
    ///
    ///     POST /TakeSlot
    ///     {
    ///         "facilityId": "Guid",
    ///         "start": "DateTime",
    ///         "end": "DateTime",
    ///         "patient": {
    ///             "name": "string",
    ///             "secondName": "string",
    ///             "phone": "string",
    ///             "email": "string"
    ///         },
    ///         "comments": "string"
    ///     }
    ///
    /// </remarks>
    /// <param name="slot">The Slot object containing the details of the time slot to book.</param>
    /// <returns>A success message if the slot is booked successfully, or an error message if the booking fails.</returns>
    /// <response code="200">Returns a success message if the time slot is successfully booked</response>
    /// <response code="400">If the provided Slot object is null or if booking the time slot fails due to invalid data</response>
    /// <response code="500">If an internal server error occurs while processing the request</response>
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

    private static bool IsValidDateFormat(string date, out DateTime parsedDate)
    {
        return DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out parsedDate);
    }
}