using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;
using MISSA.Services.Staging.Infrastructure;

namespace Staging.Infrastructure;

/// <remarks>
/// Add migrations using the following command inside the 'Ordering.Infrastructure' project directory:
///
/// dotnet ef migrations add --startup-project Ordering.API --context OrderingContext [migration-name]
/// </remarks>
public class DatabaseContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;
    private IDbContextTransaction _currentTransaction;

    public DbSet<Package> Packages { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        System.Diagnostics.Debug.WriteLine("ISSNContext::ctor ->" + this.GetHashCode());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("staging");

        modelBuilder.ApplyConfiguration(new PackageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventDataFlagEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventDataFlagCommentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventDeviceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventHouseholdEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventHouseholdAttributeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventHouseholdMemberEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventHouseholdMemberAttributeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventHouseholdSynchEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PackageEventHouseholdSynchMetaAttributesEntityTypeConfiguration());

        //modelBuilder.UseIntegrationEventLogs();
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch Domain Events collection. 
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
        await _mediator.DispatchDomainEventsAsync(this);

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        _ = await base.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}

#nullable enable
