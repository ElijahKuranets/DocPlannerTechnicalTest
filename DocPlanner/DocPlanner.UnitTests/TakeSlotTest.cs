using DocPlanner.Models;
using DocPlanner.Services;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocPlanner.UnitTests
{
    public class TakeSlotTest
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();
        private readonly Mock<ILogger<SlotService>> _mockLogger = new();

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
}
