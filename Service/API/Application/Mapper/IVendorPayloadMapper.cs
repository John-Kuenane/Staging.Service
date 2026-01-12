namespace Staging.API.Application.Mapper;

public interface IVendorPayloadMapper
{
    object Map(string vendor, object domainObject);
}
