using DocPlanner.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DocPlanner.UnitTests.Services;

public class HttpContextAccessorServiceTests
{
    [Fact]
    public void GetCurrentHttpContext_WhenContextAvailable_ReturnsCorrectContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var httpContextAccessorService = new HttpContextAccessorService(httpContextAccessorMock.Object);

        // Act
        var result = httpContextAccessorService.GetCurrentHttpContext();

        // Assert
        result.Should().BeSameAs(httpContext);
    }

    [Fact]
    public void GetCurrentHttpContext_WhenNoContextAvailable_ReturnsNull()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

        var httpContextAccessorService = new HttpContextAccessorService(httpContextAccessorMock.Object);

        // Act
        var result = httpContextAccessorService.GetCurrentHttpContext();

        // Assert
        result.Should().BeNull();
    }
}