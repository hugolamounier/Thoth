using System.Threading.Tasks;

namespace Thoth.Interfaces;

public interface IDatabase
{
    Task<bool> IsEnabledAsync(string featureName);
}