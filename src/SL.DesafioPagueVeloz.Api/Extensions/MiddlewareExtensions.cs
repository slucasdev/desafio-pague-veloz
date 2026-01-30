namespace SL.DesafioPagueVeloz.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseCustomMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseRateLimiter();
        app.UseAuthorization();

        return app;
    }
}