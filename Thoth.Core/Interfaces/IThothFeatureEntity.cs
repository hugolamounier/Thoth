using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thoth.Core.Interfaces;

public interface IThothFeatureEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    Task<bool> IsValidAsync(out List<string> messages);
}