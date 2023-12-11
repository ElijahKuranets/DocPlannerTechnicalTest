using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Text;
using DocPlanner.Interfaces;

namespace DocPlanner.UnitTests;

public class BasicAuthenticationHandlerTests
{
    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(x => x.CheckUser("testuser", "testpassword")).Returns(true);

        var options = new AuthenticationSchemeOptions();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var urlEncoderMock = new Mock<UrlEncoder>();
        var systemClockMock = new Mock<ISystemClock>();

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Basic " + Convert.ToBase64String("testuser:testpassword"u8.ToArray());

        var authenticationScheme = new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler));

        var handler = new TestableBasicAuthenticationHandler(
            userServiceMock.Object,
            optionsMonitorMock.Object,
            loggerFactoryMock.Object,
            urlEncoderMock.Object,
            systemClockMock.Object);

        // Initializing the handler with the context and scheme
        await handler.InitializeAsync(authenticationScheme, context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    private class TestableBasicAuthenticationHandler : BasicAuthenticationHandler
    {
        public TestableBasicAuthenticationHandler(
            IUserService userService,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(userService, options, logger, encoder, clock)
        {
        }

        public Task<AuthenticateResult> AuthenticateAsync() => HandleAuthenticateAsync();
    }
}
