using System;
using System.Threading.Tasks;
using Thoth.Models;

namespace Thoth.Interfaces;

public interface IDatabase : IDisposable
{
    Task<bool> IsEnabledAsync(string featureFlagName);
    Task<bool> AddAsync(FeatureFlag featureFlag);
    Task<bool> UpdateAsync(string featureFlagName, bool value, string filterValue);
    Task<bool> DeleteAsync(string featureFlagName);
    Task<bool> ExistsAsync(string featureFlagName);
}