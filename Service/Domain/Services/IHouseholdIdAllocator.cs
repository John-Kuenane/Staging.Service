namespace Staging.Domain.Services;

public interface IHouseholdIdAllocator
{
    Task<int> GetNextHouseholdIdAsync(CancellationToken cancellationToken = default);
}