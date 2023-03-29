using System.Threading.Tasks;
using Thoth.Models;

namespace Thoth.Interfaces;

public interface IFeatureFlagManagement
{
    Task<bool> AddAsync(string name, FeatureFlagsTypes type, bool value);
    Task UpdateAsync(string name, bool value);
    Task DeleteAsync(string name);
}