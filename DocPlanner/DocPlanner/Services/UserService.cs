using DocPlanner.Interfaces;
using DocPlanner.Models;
using Microsoft.Extensions.Options;

namespace DocPlanner.Services;

public class UserService : IUserService
{
    private readonly List<UserCredentials> _userCredentials;

    public UserService(IOptions<List<UserCredentials>> userCredentialsOptions)
    {
        _userCredentials = userCredentialsOptions.Value;
    }

    public bool CheckUser(string username, string password)
    {
        return _userCredentials.Any(uc => uc.Username.Equals(username) && uc.Password.Equals(password));
    }
}