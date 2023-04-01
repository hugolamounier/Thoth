using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Core.Models;

namespace Thoth.Core.Interfaces;

public interface IThothFeatureManager
{
    Task<bool> IsEnabledAsync(string name);
    Task<bool> IsActiveAsync(string name);
    Task<FeatureFlag> GetAsync(string name);
    Task<IEnumerable<FeatureFlag>> GetAllAsync();
    Task<bool> AddAsync(FeatureFlag featureFlag);
    Task<bool> UpdateAsync(FeatureFlag featureFlag);
    Task<bool> DeleteAsync(string name);
}