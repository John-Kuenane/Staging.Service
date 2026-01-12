using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

/// <remarks>
/// Add migrations using the following command inside the 'Staging.Infrastructure' project directory:
///
/// dotnet ef migrations add --startup-project Staging.API --context StagingContext [migration-name]
/// </remarks>
public class DatabaseContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;
    private IDbContextTransaction _currentTransaction;

    public DbSet<ExternalSubmissionAggregate> ExternalSubmissions { get; set; } = default!;
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageEvent> PackageEvents { get; set; }
    public DbSet<PackageEventHousehold> PackageEventHouseholds { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IMediator? mediator = null)
        : base(options)
    {
        _mediator = mediator;

        System.Diagnostics.Debug.WriteLine("StagingContext::ctor ->" + this.GetHashCode());
    }

    public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("staging");

        modelBuilder.ApplyConfiguration(new ExternalSubmissionEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FormCategoryEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FormElementAttributeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FormElementDependencyEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FormElementEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FormEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FormVersionEntityTypeConfiguration());
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
        ChangeTracker.DetectChanges();
        var timestamp = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            try
            {
                //var userName = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //var currentUser = Users.SingleOrDefault(u => u.UserName == userName);

                entry.Property("LastModified").CurrentValue = timestamp;
                entry.Property("LastModifiedId").CurrentValue = Guid.Parse("FB942BF5-97EE-4048-88E6-245ABA257B28");

                if (entry.State == EntityState.Added)
                {
                    //entry.Property("Id").CurrentValue = Guid.NewGuid();
                    entry.Property("Created").CurrentValue = timestamp;
                    entry.Property("CreatedId").CurrentValue = Guid.Parse("FB942BF5-97EE-4048-88E6-245ABA257B28");
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        _ = await base.SaveChangesAsync(cancellationToken);

        // Dispatch Domain Events collection. 
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
        if (_mediator != null)
        {
            await _mediator.DispatchDomainEventsAsync(this);
        }

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
