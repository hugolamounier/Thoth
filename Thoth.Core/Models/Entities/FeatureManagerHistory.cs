using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thoth.Core.Models.Entities;

public class FeatureManagerHistory : BaseFeatureManager
{
    public FeatureManagerHistory(BaseFeatureManager featureManager)
    {
        Name = featureManager.Name;
        Type = featureManager.Type;
        SubType = featureManager.SubType;
        Enabled = featureManager.Enabled;
        Value = featureManager.Value;
        Description = featureManager.Description;
        Extras = featureManager.Extras;
        CreatedAt = featureManager.CreatedAt;
        UpdatedAt = featureManager.UpdatedAt;
        PeriodStart = featureManager.UpdatedAt ?? featureManager.CreatedAt;
        PeriodEnd = DateTime.UtcNow;
    }
    
    public DateTime PeriodEnd { get; set; }
    public DateTime PeriodStart { get; set; }
    
    public override Task<bool> IsValidAsync(out List<string> messages)
    {
        messages = new List<string>();
        return Task.FromResult(true);
    }
}