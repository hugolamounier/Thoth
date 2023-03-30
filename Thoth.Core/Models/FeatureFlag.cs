using System;

namespace Thoth.Core.Models;

public class FeatureFlag
{
    public string Name { get; set; }
    public FeatureFlagsTypes Type { get; set; }
    public bool Value { get; set; }
    public string FilterValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}