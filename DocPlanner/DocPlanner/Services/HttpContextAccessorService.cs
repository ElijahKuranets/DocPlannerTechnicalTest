using DocPlanner.Interfaces;

namespace DocPlanner.Services;

public class HttpContextAccessorService : IHttpContextAccessorService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpContextAccessorService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public HttpContext GetCurrentHttpContext()
    {
        return _contextAccessor.HttpContext;
    }
}