namespace DocPlanner.Interfaces;

public interface IUserService
{
    bool CheckUser(string username, string password);
}