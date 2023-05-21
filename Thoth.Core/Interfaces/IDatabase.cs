#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Core.Models.Entities;

namespace Thoth.Core.Interfaces;

public interface IDatabase
{
    Task<FeatureManager> GetAsync(string featureName);
    Task<IEnumerable<FeatureManager>> GetAllAsync();
    Task<IEnumerable<FeatureManager>> GetAllDeletedAsync();
    Task<bool> AddAsync(FeatureManager featureManager);
    Task<bool> UpdateAsync(FeatureManager featureManager);
    Task<bool> DeleteAsync(string featureName, string auditExtras = "");
    Task<bool> ExistsAsync(string featureName);
}