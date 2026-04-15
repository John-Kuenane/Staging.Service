using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Staging.Domain.Services;
using System.Data;

namespace Staging.API.Infrastructure.Services;

public class HouseholdIdAllocator : IHouseholdIdAllocator
{
    private readonly DatabaseContext _context;

    public HouseholdIdAllocator(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextHouseholdIdAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR [staging].[HouseholdIdSequence];";
        command.CommandType = CommandType.Text;

        var currentTransaction = _context.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            command.Transaction = currentTransaction.GetDbTransaction();
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException("Unable to allocate the next household ID from staging.HouseholdIdSequence.");
        }

        return Convert.ToInt32(result);
    }
}