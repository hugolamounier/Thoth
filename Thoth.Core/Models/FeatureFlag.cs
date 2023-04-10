using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Thoth.Core.Models;

public class FeatureFlag: IEntity
{
    public string Name { get; set; }
    public FeatureFlagsTypes Type { get; set; }
    public bool Value { get; set; }
    public string FilterValue { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Task<bool> IsValidAsync(out List<string> messages)
    {
        messages = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            messages.Add(string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(Name)));
        
        if (Type == FeatureFlagsTypes.PercentageFilter && string.IsNullOrWhiteSpace(FilterValue))
            messages.Add(string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(FilterValue)));

        return Task.FromResult(!messages.Any());
    }
}