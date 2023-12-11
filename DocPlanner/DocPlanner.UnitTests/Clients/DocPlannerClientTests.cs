using System.Net;
using DocPlanner.Clients;
using DocPlanner.Models;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace DocPlanner.UnitTests.Clients;

public class DocPlannerClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly DocPlannerClient _docPlannerClient;
    private readonly SlotServiceApiConfig _apiConfig;

    public DocPlannerClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _docPlannerClient = new DocPlannerClient(_httpClient);
        _apiConfig = new SlotServiceApiConfig()
        {
            BaseUrl = "http://test.com"
        };
    }

    [Fact]
    public void Constructor_WhenCalled_SetsHttpClient()
    {
        // Arrange & Act done in constructor

        // Assert
        _docPlannerClient.Client.Should().BeSameAs(_httpClient);
    }

    [Fact]
    public async Task MakeRequest_WhenCalled_ReturnsExpectedResponse()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test response")
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var response = await _docPlannerClient.Client.GetAsync(_apiConfig.BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Test response");
    }
}