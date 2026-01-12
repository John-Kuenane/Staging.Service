namespace Staging.API.Application.Mapper;

public class VendorPayloadMapper : IVendorPayloadMapper
{
    private readonly IServiceProvider _serviceProvider;

    public VendorPayloadMapper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object Map(string vendor, object domainObject)
    {
        var domainType = domainObject.GetType();
        var mapperType = typeof(IVendorMapper<>).MakeGenericType(domainType);
        var mapper = _serviceProvider.GetService(mapperType);
        if (mapper == null)
            throw new InvalidOperationException($"No mapper registered for {vendor} + {domainType.Name}");

        var mapMethod = mapperType.GetMethod("Map");
        return mapMethod.Invoke(mapper, new[] { domainObject })!;
    }
}
