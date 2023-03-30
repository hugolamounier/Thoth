using System.Threading.Tasks;
using Thoth.Core.Models;

namespace Thoth.Core.Interfaces;

public interface IFeatureFlagManagement
{
    Task<bool> IsEnabledAsync(string name);
    Task<FeatureFlag> GetAsync(string name);
    Task<bool> AddAsync(FeatureFlag featureFlag);
    Task<bool> UpdateAsync(FeatureFlag featureFlag);
    Task<bool> DeleteAsync(string name);
}