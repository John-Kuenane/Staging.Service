namespace Staging.Domain.AggregatesModel.FormAggregate;

//This is just the RepositoryContracts or Interface defined at the Domain Layer

public interface IFormRepository : IRepository<Form>
{
    Task<Form> GetAsync(Guid id);
}
