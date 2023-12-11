using DocPlanner.Models;
using DocPlanner.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DocPlanner.UnitTests.Services;

public class UserServiceTests
{
    private readonly UserService _userService;

    private readonly List<UserCredentials> _credentialsList = new()
    {
        new UserCredentials() {Username = "user1", Password = "pass1"},
        new UserCredentials() {Username = "user2", Password = "pass2"}
    };

    public UserServiceTests()
    {
        var optionsMock = new Mock<IOptions<List<UserCredentials>>>();
        optionsMock.Setup(o => o.Value).Returns(_credentialsList);
        _userService = new UserService(optionsMock.Object);
    }

    [Theory]
    [InlineData("user1", "pass1", true)]
    [InlineData("user2", "pass2", true)]
    [InlineData("user3", "pass3", false)]
    [InlineData("user1", "wrongpass", false)]
    [InlineData("wronguser", "pass1", false)]
    public void CheckUser_ValidatesUserCredentialsCorrectly(string username, string password, bool expectedResult)
    {
        // Act
        var result = _userService.CheckUser(username, password);

        // Assert
        result.Should().Be(expectedResult);
    }
}