using DocPlanner.Controllers;
using DocPlanner.Interfaces;
using DocPlanner.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocPlanner.UnitTests.Controllers;

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
    public async Task GetWeeklySlots_WhenExceptionOccurs_ReturnsStatusCode500()
    {
        // Arrange
        const string testDate = "20231201";
        _mockSlotService.Setup(s => s.GetWeeklySlotsAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetWeeklySlots(testDate);

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
    
    [Fact]
    public async Task GetWeeklyAvailability_WithValidDate_ReturnsAvailabilityData()
    {
        // Arrange
        const string testDate = "20231207";
        var expectedResponse = new AvailabilityResponse();
        _mockSlotService.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetWeeklyAvailability(testDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetWeeklyAvailability_WithValidDate_ReturnData_ReturnsOk()
    {
        // Arrange
        const string testDate = "20231207";
        _mockSlotService.Setup(s => s.GetWeeklyAvailabilityAsync(It.IsAny<DateTime>()))!
            .ReturnsAsync(new AvailabilityResponse()
            {
                Facility = new Facility()
                {
                    FacilityId = Guid.NewGuid(),
                    Name = "testName",
                    Address = "testName"
                }
            });

        // Act
        var result = await _controller.GetWeeklyAvailability(testDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetWeeklySlots_WithValidDate_ReturnsSlotsList()
    {
        // Arrange
        const string testDate = "20231207";
        var expectedSlots = new List<TimeSlot>()
        {
            new()
            {
                Start = new DateTime(),
                End = new DateTime()
            }
        }; 
        _mockSlotService.Setup(s => s.GetWeeklySlotsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(expectedSlots);

        // Act
        var result = await _controller.GetWeeklySlots(testDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSlots);
    }

    [Fact]
    public async Task GetWeeklySlots_WithValidDate_AvailableSlots_ReturnsOk()
    {
        // Arrange
        const string testDate = "20231207";
        _mockSlotService.Setup(s => s.GetWeeklySlotsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TimeSlot>()
            {
                new ()
            });

        // Act
        var result = await _controller.GetWeeklySlots(testDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    
    [Fact]
    public async Task GetWeeklySlots_WithValidDate_NoSlots_ReturnsNotFound()
    {
        // Arrange
        const string testDate = "20231207";
        _mockSlotService.Setup(s => s.GetWeeklySlotsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TimeSlot>());

        // Act
        var result = await _controller.GetWeeklySlots(testDate);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task TakeSlot_WithInvalidSlot_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await _controller.TakeSlot(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task TakeSlot_BookingFails_ReturnsBadRequest()
    {
        // Arrange
        var slot = new Slot()
        {
            FacilityId = Guid.NewGuid(),
            Comments = "test comments",
            Start = new DateTime(),
            End = new DateTime(),
            Patient = new Patient
            {
                Email = "test@gmail.com",
                Name = "testName",
                SecondName = "testSecondName",
                Phone = "111222333"
            }
        };
        _mockSlotService.Setup(s => s.TakeSlotAsync(It.IsAny<Slot>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.TakeSlot(slot);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task TakeSlot_WithException_ReturnsStatusCode500()
    {
        // Arrange
        var slot = new Slot();
        _mockSlotService.Setup(s => s.TakeSlotAsync(It.IsAny<Slot>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.TakeSlot(slot);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}