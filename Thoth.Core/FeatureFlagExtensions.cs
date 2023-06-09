﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thoth.Core.Models.Entities;
using Thoth.Core.Utils;

namespace Thoth.Core;

public static class FeatureFlagExtensions
{
    public static Task<bool> EvaluatePercentageFlag(this FeatureManager featureManager,
        ILogger<ThothFeatureManager> logger)
    {
        if (Convert.ToDouble(featureManager.Value) > 0)
            return Task.FromResult(RandomGenerator.NextDouble() * 100.0 < Convert.ToDouble(featureManager.Value));

        logger.LogWarning(
            "When using 'PercentageFilter' flag type, 'Value' must be defined and be greater than zero (0) for the feature: {FeatureFlagName}",
            featureManager.Name);
        return Task.FromResult(false);
    }
}