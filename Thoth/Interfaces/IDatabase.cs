using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Models;

namespace Thoth.Interfaces;

public interface IDatabase : IDisposable
{
    Task<bool> IsEnabledAsync(string featureFlagName);
    Task<FeatureFlag> GetAsync(string name);
    Task<IEnumerable<FeatureFlag>> GetAll();
    Task<bool> AddAsync(FeatureFlag featureFlag);
    Task<bool> UpdateAsync(FeatureFlag featureFlag);
    Task<bool> DeleteAsync(string featureFlagName);
    Task<bool> ExistsAsync(string featureFlagName);
}