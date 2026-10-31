using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ShiftStudy.Api;

public static class ApiEndpoints
{
    public static IServiceCollection AddShiftStudyApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddHealthChecks();
        return services;
    }

    public static IEndpointRouteBuilder MapShiftStudyApi(this IEndpointRouteBuilder app)
    {
        app.MapControllers();
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");
        return app;
    }
}
