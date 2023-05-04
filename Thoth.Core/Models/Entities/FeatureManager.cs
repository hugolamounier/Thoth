using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Enums;

namespace Thoth.Core.Models.Entities;

public class FeatureManager: IThothFeatureEntity
{
    public string Name { get; set; }
    public FeatureTypes Type { get; set; }
    public FeatureFlagsTypes? SubType { get; set; }
    public bool Enabled { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public string Extras { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Task<bool> IsValidAsync(out List<string> messages)
    {
        messages = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            messages.Add(string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(Name)));

        if(Type == FeatureTypes.EnvironmentVariable && string.IsNullOrWhiteSpace(Value))
            messages.Add(string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(Value)));

        if(Type == FeatureTypes.EnvironmentVariable && SubType != null)
            messages.Add(string.Format(Messages.VALIDATION_NO_SUB_TYPE, "EnvironmentVariable"));

        if(Type == FeatureTypes.FeatureFlag && SubType is null)
            messages.Add(string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(SubType)));

        if (SubType == FeatureFlagsTypes.PercentageFilter && string.IsNullOrWhiteSpace(Value))
            messages.Add(string.Format(Messages.VALIDATION_INVALID_FIELD, nameof(Value)));

        if (SubType == FeatureFlagsTypes.Boolean && !string.IsNullOrWhiteSpace(Value))
            messages.Add(Messages.ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_VALUE);

        return Task.FromResult(!messages.Any());
    }
}