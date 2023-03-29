using System;
using System.Data;
using System.Threading.Tasks;
using Thoth.Interfaces;

namespace Thoth.SQLServer;

public class SqlServerDatabase : IDatabase, IDisposable
{
    private readonly IDbConnection _dbConnection;

    public SqlServerDatabase(string sqlConnectionString)
    {
        _dbConnection = new SqlConnection();
    }

    public Task<bool> IsEnabledAsync(string featureName)
    {
        throw new NotImplementedException();
    }

    private void DatabaseExists(TContext dbContext)
    {
        var metaData = dbContext.Model.FindEntityType(typeof());
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}