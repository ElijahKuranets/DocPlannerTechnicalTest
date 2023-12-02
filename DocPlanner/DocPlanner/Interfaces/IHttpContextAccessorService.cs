namespace DocPlanner.Interfaces;

public interface IHttpContextAccessorService
{
    HttpContext GetCurrentHttpContext();
}