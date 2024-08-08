namespace MyZhiHuAPI.Middleware;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyAuthorize(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddMyAuthorize(this IServiceCollection services,
        Action<MyAuthorizeOption> configure)
    {
        var option = new MyAuthorizeOption();
        configure(option);
        services.Configure(configure);
        return services.AddMyAuthorize();
    }
}
