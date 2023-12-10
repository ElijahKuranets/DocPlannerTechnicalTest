using DocPlanner.Controllers;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocPlanner.UnitTests;

public class AvailabilityControllerTests
{
    private readonly AvailabilityController _controller;
    private readonly Mock<ISlotService> _mockSlotService;

    public AvailabilityControllerTests()
    {
        _mockSlotService = new Mock<ISlotService>(); 
        Mock<ILogger<AvailabilityController>> mockLogger = new();
        _controller = new AvailabilityController(_mockSlotService.Object, mockLogger.Object);
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
        const string testDate = "20231201";
        _mockSlotService.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetWeeklyAvailability(testDate);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
    
    [Fact]
    public async Task TakeSlot_WithValidSlot_ReturnsSuccessMessage()
    {
        // Arrange
        var slot = new Slot
        {
            FacilityId = Guid.NewGuid(),
            Start= new DateTime(),
            End = new DateTime(),
            Comments = "comments",
            Patient = new Patient
            {
                Name = "patientsTestName",
                SecondName = "patientsTestSecondName",
                Email = "patientsTestEmail@gmail.com",
                Phone = "111 111 111"
            }
        };
        _mockSlotService.Setup(s => s.TakeSlotAsync(It.IsAny<Slot>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TakeSlot(slot);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be("TimeSlot successfully booked.");
    }
}