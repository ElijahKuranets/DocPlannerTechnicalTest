using System.Net;
using System.Text;
using DocPlanner.Models;
using DocPlanner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace DocPlanner.UnitTests;

public class WeeklyAvailabilityTest
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
                FacilityId = Guid.NewGuid(),
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

    private static HttpClient SetupHttpClient(HttpResponseMessage response)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler(response);
        return new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri("http://testlocalhost/"),
        };
    }
}