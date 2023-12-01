using DocPlanner.Models;
using DocPlanner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace DocPlanner.UnitTests
{
    public class WeeklySlotsTest
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();
        private readonly Mock<ILogger<SlotService>> _mockLogger = new();

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
        private static HttpClient SetupHttpClient(HttpResponseMessage response)
        {
            var mockHttpMessageHandler = new MockHttpMessageHandler(response);
            return new HttpClient(mockHttpMessageHandler)
            {
                BaseAddress = new Uri("http://testlocalhost/"),
            };
        }
    }
}
