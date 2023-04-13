#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Core.Models.Entities;

namespace Thoth.Core.Interfaces;

public interface IDatabase : IDisposable
{
    Task<FeatureManager?> GetAsync(string featureName);
    Task<List<FeatureManager>> GetAllAsync();
    Task<bool> AddAsync(FeatureManager featureManager);
    Task<bool> UpdateAsync(FeatureManager featureManager);
    Task<bool> DeleteAsync(string featureName);
    Task<bool> ExistsAsync(string featureName);
}