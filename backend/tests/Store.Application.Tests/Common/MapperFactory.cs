using AutoMapper;

namespace Store.Application.Tests.Common;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(Store.Application.DependencyInjection).Assembly));

        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }
}
