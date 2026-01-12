namespace Staging.API.Application.Mapper;

public interface IVendorMapper<TDomain>
{
    object Map(TDomain domainObject);
}
