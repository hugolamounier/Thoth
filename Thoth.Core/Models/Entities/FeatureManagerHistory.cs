using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Enums;

namespace Thoth.Core.Models.Entities;

public class FeatureManagerHistory : IThothFeatureEntity
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

        return Task.FromResult(true);
    }
}