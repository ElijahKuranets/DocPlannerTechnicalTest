using System.Net;
using DocPlanner.Clients;
using DocPlanner.Models;
using DocPlanner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace DocPlanner.UnitTests.Services;

public class SlotServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly SlotService _slotService;

    public SlotServiceTests()
    {
        // Mock the HttpMessageHandler to control the HttpClient's behavior
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        // Create HttpClient using the mocked HttpMessageHandler
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object){
            BaseAddress = new Uri("http://example.com/")
        };;
        var docPlannerClient = new DocPlannerClient(httpClient);

        // Mock the ILogger
        Mock<ILogger<SlotService>> loggerMock = new();

        // Create an instance of SlotService with mocked dependencies
        _slotService = new SlotService(docPlannerClient, loggerMock.Object);
    }
    
    [Fact]
    public async Task GetWeeklyAvailabilityAsync_WhenSuccessful_ReturnsAvailability()
    {
        // Arrange: Set up the expected date and mock response
        var date = DateTime.UtcNow;
        var availabilityResponse = new AvailabilityResponse()
        {
            Facility = new Facility()
            {
                FacilityId = Guid.NewGuid(),
                Name = "testName",
                Address = "testAddress"
            },
            Monday = new WeekDay()
            {
                WorkPeriod = new WorkPeriod()
                {
                    BusySlots = new List<TimeSlot>()
                    {
                        new ()
                        {
                            Start = new DateTime(),
                            End = new DateTime()
                        }
                    },
                    StartHour = 8,
                    EndHour = 16,
                    LunchStartHour = 11,
                    LunchEndHour = 12
                }
            },
            Tuesday = new WeekDay(),
            Wednesday = new WeekDay(),
            Thursday = new WeekDay(),
            Friday = new WeekDay(),
            Saturday = new WeekDay(),
            Sunday = new WeekDay(),
            SlotDurationMinutes = 20,
        };
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(availabilityResponse))
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("GetWeeklyAvailability")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act: Call the method
        var result = await _slotService.GetWeeklyAvailabilityAsync(date);

        // Assert: Check if the result is as expected
        result.Should().BeEquivalentTo(availabilityResponse);
    }
    
    [Fact]
    public async Task GetWeeklySlotsAsync_WhenSuccessful_ReturnsSlots()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var slotsResponse = new List<TimeSlot>()
        {
            new ()
            {
                Start = new DateTime(),
                End = new DateTime(),
            }
        };
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(slotsResponse))
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("GetWeeklySlots")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _slotService.GetWeeklySlotsAsync(date);

        // Assert
        result.Should().BeEquivalentTo(slotsResponse);
    }
    
    [Fact]
    public async Task TakeSlotAsync_WhenSuccessful_ReturnsTrue()
    {
        // Arrange
        var slot = new Slot()
        {
            FacilityId = Guid.NewGuid(),
            Comments = "test comments",
            Start = new DateTime(),
            End = new DateTime(),
            Patient = new Patient()
            {
                Email = "test@gmail.com",
                Name = "testName",
                Phone = "111222333",
                SecondName = "testSecondName"
            }
        };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString().Contains("TakeSlot")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _slotService.TakeSlotAsync(slot);

        // Assert
        result.Should().BeTrue();
    }
}