using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.SQLServer;

public class ThothSqlServerProvider : IDatabase
{
    private const string SchemaName = "thoth";
    private readonly IDbConnection _dbConnection;

    public ThothSqlServerProvider(string connectionString)
    {
        _dbConnection = new SqlConnection(connectionString);
        Init();
    }

    public async Task<FeatureManager> GetAsync(string featureName)
    {
        return await _dbConnection
            .QueryFirstAsync<FeatureManager>(string.Format(Queries.GetQuery, SchemaName),
                new {Name = featureName});
    }

    public async Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        return await _dbConnection.QueryAsync<FeatureManager>(string.Format(Queries.GetAllQuery, SchemaName));
    }

    public async Task<bool> AddAsync(FeatureManager featureManager)
    {
        featureManager.CreatedAt = DateTime.UtcNow;
        return await _dbConnection.ExecuteAsync(string.Format(Queries.AddFeatureManagerQuery, SchemaName), featureManager) > 0;
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        featureManager.UpdatedAt = DateTime.UtcNow;
        return await _dbConnection.ExecuteAsync(string.Format(Queries.UpdateFeatureManager, SchemaName), featureManager) > 0;
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        return await _dbConnection.ExecuteAsync(string.Format(Queries.DeleteFeatureManagerQuery, SchemaName), new {Name = featureName}) > 0;
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await _dbConnection.QueryFirstOrDefaultAsync<FeatureManager?>(string.Format(Queries.GetQuery, SchemaName),
            new {Name = featureName}) is not null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        _dbConnection.Close();
        _dbConnection.Dispose();
    }

    private void Init()
    {
        _dbConnection.Open();

        _dbConnection.Execute(string.Format(Queries.CreateSchemaIfNotExistsQuery, SchemaName));
        _dbConnection.Execute(string.Format(Queries.CreateFeatureManagerTableQuery, SchemaName));
    }
}