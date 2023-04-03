using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thoth.Core.Models;

public interface IEntity
{
    Task<bool> IsValidAsync(out List<string> messages);
}