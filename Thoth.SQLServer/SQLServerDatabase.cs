using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Thoth.Interfaces;
using Thoth.Models;

namespace Thoth.SQLServer;

public class SqlServerDatabase : IDatabase
{
    private readonly IDbConnection _dbConnection;
    private const string SchemaName = "thoth";

    public SqlServerDatabase(string sqlConnectionString)
    {
        _dbConnection = new SqlConnection(sqlConnectionString);
        Init();
    }

    public Task<bool> IsEnabledAsync(string featureFlagName) =>
        _dbConnection.QueryFirstOrDefaultAsync<bool>(string.Format(Queries.IsEnabledQuery, SchemaName),
            new {Name = featureFlagName});

    public async Task<bool> AddAsync(FeatureFlag featureFlag) =>
       await _dbConnection.ExecuteAsync(string.Format(Queries.AddFeatureFlagQuery, SchemaName), featureFlag) > 0;

    public async Task<bool> UpdateAsync(string featureFlagName, bool value, string filterValue) =>
        await _dbConnection.ExecuteAsync(string.Format(Queries.UpdateFeatureFlag, SchemaName), new
        {
            Name = featureFlagName,
            Value = value,
            FilterValue = filterValue,
            UpdatedAt = DateTime.UtcNow
        }) > 0;

    public async Task<bool> DeleteAsync(string featureFlagName) =>
        await _dbConnection.ExecuteAsync(string.Format(Queries.DeleteFeatureFlagQuery, new { Name = featureFlagName })) > 0;

    public async Task<bool> ExistsAsync(string featureFlagName) =>
        await _dbConnection.QueryFirstOrDefaultAsync<bool?>(string.Format(Queries.IsEnabledQuery, SchemaName),
            new {Name = featureFlagName}) != null;

    public void Init()
    {
        _dbConnection.Open();

        _dbConnection.Execute(string.Format(Queries.CreateSchemaIfNotExistsQuery, SchemaName));
        _dbConnection.Execute(string.Format(Queries.CreateFeatureFlagTableQuery, SchemaName));
    }

    public void Dispose()
    {
       _dbConnection.Dispose();
       GC.SuppressFinalize(this);
    }
}