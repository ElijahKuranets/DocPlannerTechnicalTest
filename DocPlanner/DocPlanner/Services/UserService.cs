using DocPlanner.Interfaces;

namespace DocPlanner.Services
{
    public class UserService : IUserService
    {
        public bool CheckUser(string username, string password)
        {
            return username.Equals("techuser") && password.Equals("passWord") || username.Equals("mamerto") && password.Equals("mellon");
        }
    }
}
