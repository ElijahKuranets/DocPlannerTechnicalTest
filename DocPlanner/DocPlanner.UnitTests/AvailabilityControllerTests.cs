using DocPlanner.Controllers;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocPlanner.UnitTests;

public class WeeklyAvailabilityTest
{
    private readonly AvailabilityController _controller;
    private readonly Mock<ISlotService> mockSlotService;
    private readonly Mock<ILogger<AvailabilityController>> mockLogger;
    
    public WeeklyAvailabilityTest() //AvailabilityControllerTests()
    {
        mockSlotService = new Mock<ISlotService>(); 
        mockLogger = new Mock<ILogger<AvailabilityController>>();
        _controller = new AvailabilityController(mockSlotService.Object, mockLogger.Object);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("202-12-01")]
    [InlineData("invalid-date")]
    public async Task GetWeeklyAvailability_WithInvalidDateFormat_ReturnsBadRequest(string testDate)
    {
        // Act
        var result = await _controller.GetWeeklyAvailability(testDate);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid date format. Please use yyyyMMdd format.");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("202-12-01")]
    [InlineData("invalid-date")]
    public async Task GetWeeklySlots_WithInvalidDateFormat_ReturnsBadRequest(string testDate)
    {
        // Act
        var result = await _controller.GetWeeklySlots(testDate);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task GetWeeklyAvailability_WhenExceptionOccurs_ReturnsStatusCode500()
    {
        // Arrange
        var testDate = "20231201";
        mockSlotService.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetWeeklyAvailability(testDate);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
    }
    
    [Fact]
    public async Task TakeSlot_WithValidSlot_ReturnsSuccessMessage()
    {
        // Arrange
        var slot = new Slot { /* Populate with valid data */ };
        mockSlotService.Setup(s => s.TakeSlotAsync(It.IsAny<Slot>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TakeSlot(slot);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().Be("TimeSlot successfully booked.");
    }
}