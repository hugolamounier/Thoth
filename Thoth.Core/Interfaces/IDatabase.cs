#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Core.Models.Entities;

namespace Thoth.Core.Interfaces;

public interface IDatabase
{
    Task<FeatureManager> GetAsync(string featureName);
    Task<IEnumerable<FeatureManager>> GetAllAsync();
    Task<bool> AddAsync(FeatureManager featureManager);
    Task<bool> UpdateAsync(FeatureManager featureManager);
    Task<bool> DeleteAsync(string featureName);
    Task<bool> ExistsAsync(string featureName);
}