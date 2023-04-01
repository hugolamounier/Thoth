using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.SQLServer;

public class SqlServerDatabase : IDatabase
{
    private readonly IDbConnection _dbConnection;
    private const string SchemaName = "thoth";

    public SqlServerDatabase(IOptions<ThothOptions> options)
    {
        _dbConnection = new SqlConnection(options.Value.ConnectionString);
        Init();
    }

    public Task<bool> IsEnabledAsync(string featureFlagName) =>
        _dbConnection.QueryFirstOrDefaultAsync<bool>(string.Format(Queries.IsEnabledQuery, SchemaName),
            new {Name = featureFlagName});

    public Task<FeatureFlag> GetAsync(string name) =>
        _dbConnection.QueryFirstAsync<FeatureFlag>(string.Format(Queries.GetQuery, SchemaName), new {Name = name});

    public Task<IEnumerable<FeatureFlag>> GetAllAsync() =>
        _dbConnection.QueryAsync<FeatureFlag>(string.Format(Queries.GetAllQuery, SchemaName));

    public async Task<bool> AddAsync(FeatureFlag featureFlag) =>
       await _dbConnection.ExecuteAsync(string.Format(Queries.AddFeatureFlagQuery, SchemaName), featureFlag) > 0;

    public async Task<bool> UpdateAsync(FeatureFlag featureFlag) =>
        await _dbConnection.ExecuteAsync(string.Format(Queries.UpdateFeatureFlag, SchemaName), new
        {
            featureFlag.Name,
            featureFlag.Value,
            featureFlag.FilterValue,
            UpdatedAt = DateTime.UtcNow
        }) > 0;

    public async Task<bool> DeleteAsync(string featureFlagName) =>
        await _dbConnection.ExecuteAsync(string.Format(Queries.DeleteFeatureFlagQuery, SchemaName), new { Name = featureFlagName }) > 0;

    public async Task<bool> ExistsAsync(string featureFlagName) =>
        await _dbConnection.QueryFirstOrDefaultAsync<bool?>(string.Format(Queries.IsEnabledQuery, SchemaName),
            new {Name = featureFlagName}) != null;

    private void Init()
    {
        _dbConnection.Open();

        _dbConnection.Execute(string.Format(Queries.CreateSchemaIfNotExistsQuery, SchemaName));
        _dbConnection.Execute(string.Format(Queries.CreateFeatureFlagTableQuery, SchemaName));
    }

    public void Dispose()
    {
       _dbConnection.Close();
       _dbConnection.Dispose();
       GC.SuppressFinalize(this);
    }
}