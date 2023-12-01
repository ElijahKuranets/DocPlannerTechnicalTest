using System.Net;
using System.Text;
using DocPlanner.Models;
using DocPlanner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace DocPlanner.UnitTests;

public class SlotServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();
    private readonly Mock<ILogger<SlotService>> _mockLogger = new();

    [Fact]
    public async Task GetWeeklyAvailabilityAsync_ReturnsAvailability_OnSuccess()
    {
        // Arrange
        var availabilityResponse = new AvailabilityResponse()
        {
            Facility = new Facility()
            {
                Address = "test_address",
                Name = "test_name"
            },
            SlotDurationMinutes = 10,
            Monday = new WeekDay()
            {
                WorkPeriod = new WorkPeriod()
                {
                    StartHour = 8,
                    EndHour = 15,
                    LunchStartHour = 13,
                    LunchEndHour = 14,
                    BusySlots = new List<TimeSlot>()
                    {
                        new ()
                        {
                            Start = new DateTime(2023,06,20,11,10,00),
                            End = new DateTime(2023,06,20,11,20,00)
                        }
                    }
                }
            },
        };
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(availabilityResponse), Encoding.UTF8, "application/json")
        };

        var baseAddress = new Uri("http://testlocalhost/");
        var mockHttpMessageHandler = new MockHttpMessageHandler(httpResponse);
        
        var httpClient = SetupHttpClient(httpResponse);
        _mockHttpClientFactory.Setup(f => f.CreateClient("Base64AuthClient")).Returns(httpClient);

        var slotService = new SlotService(_mockHttpClientFactory.Object, _mockLogger.Object );

        // Act
        var result = await slotService.GetWeeklyAvailabilityAsync(DateTime.Now);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(availabilityResponse, options => options.ComparingByMembers<AvailabilityResponse>());

        // Verify Logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Requesting weekly availability for date")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }


    [Fact]
    public async Task GetWeeklyAvailabilityAsync_HandlesMalformedJson()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("This is not JSON", Encoding.UTF8, "application/json")
        };

        var httpClient = SetupHttpClient(httpResponse);
        _mockHttpClientFactory.Setup(f => f.CreateClient("Base64AuthClient")).Returns(httpClient);
        var slotService = new SlotService(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act & Assert
        var act = () => slotService.GetWeeklyAvailabilityAsync(DateTime.Now);
        await act.Should().ThrowAsync<JsonReaderException>();
    }

    [Fact]
    public async Task GetWeeklySlotsAsync_ReturnsSlots_OnSuccess()
    {
        // Arrange
        var availableSlots = new List<TimeSlot>(){
            new ()
                {
                    Start = new DateTime(2023,06,20,11,10,00),
                    End = new DateTime(2023,06,20,11,20,00)
                },
            new ()
                {
                    Start = new DateTime(2023,06,20,11,20,00),
                    End = new DateTime(2023,06,20,11,30,00)
                }
        };
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(availableSlots), Encoding.UTF8, "application/json")
        };

        var httpClient = SetupHttpClient(httpResponse);
        _mockHttpClientFactory.Setup(f => f.CreateClient("Base64AuthClient")).Returns(httpClient);

        var slotService = new SlotService(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await slotService.GetWeeklySlotsAsync(DateTime.Now);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(availableSlots, options => options.ComparingByMembers<TimeSlot>());
    }

    [Fact]
    public async Task GetWeeklySlotsAsync_ReturnsNull_OnFailure()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent(string.Empty)
        };

        var httpClient = SetupHttpClient(httpResponse);
        _mockHttpClientFactory.Setup(f => f.CreateClient("Base64AuthClient")).Returns(httpClient);

        var slotService = new SlotService(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await slotService.GetWeeklySlotsAsync(DateTime.Now);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TakeSlotAsync_ReturnsFalse_OnFailure()
    {
        // Arrange
        var slotToTake = new Slot { /* Populate with necessary data */ };
        var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
        var httpClient = SetupHttpClient(httpResponse);
        _mockHttpClientFactory.Setup(f => f.CreateClient("Base64AuthClient")).Returns(httpClient);
        var slotService = new SlotService(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await slotService.TakeSlotAsync(slotToTake);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TakeSlotAsync_ReturnsTrue_OnSuccess()
    {
        // Arrange
        var slotToTake = new Slot
        {
            FacilityId = Guid.NewGuid(),
            Start = new DateTime(2023, 06, 21, 12, 20, 00),
            End = new DateTime(2023, 06, 21, 12, 20, 00),
            Comments = "broke leg for test",
            Patient = new Patient()
            {
                Name = "Harry",
                SecondName = "Tester",
                Phone = "934",
                Email = "magic_test_outside_hogwarts@gmail.com",
            }
        };

        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        var httpClient = SetupHttpClient(httpResponse);
        _mockHttpClientFactory.Setup(f => f.CreateClient("Base64AuthClient")).Returns(httpClient);

        var slotService = new SlotService(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await slotService.TakeSlotAsync(slotToTake);

        // Assert
        result.Should().BeTrue();
    }

    private static HttpClient SetupHttpClient(HttpResponseMessage response)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler(response);
        return new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri("http://testlocalhost/"),
        };
    }
}