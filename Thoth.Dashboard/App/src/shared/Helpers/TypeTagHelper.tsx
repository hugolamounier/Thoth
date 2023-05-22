import { FeatureFlagsTypes, FeatureTypes } from '../../models/featureManager';
import { Tag } from 'antd';
import React from 'react';

export default class TypeTagHelper {
  public static TagType(type: FeatureTypes, subType?: FeatureFlagsTypes) {
    switch (type) {
      case FeatureTypes.EnvironmentVariable:
        return <Tag color="gold">{FeatureTypes[type]}</Tag>;

      case FeatureTypes.FeatureFlag: {
        switch (subType) {
          case FeatureFlagsTypes.Boolean:
            return <Tag color="blue">Feature Flag: {FeatureFlagsTypes[subType]}</Tag>;

          case FeatureFlagsTypes.PercentageFilter:
            return <Tag color="purple">Feature Flag: {FeatureFlagsTypes[subType]}</Tag>;
          default:
            return <Tag color="red">Feature Flag: Unknown</Tag>;
        }
      }
      default:
        return <Tag color="red">Unknown</Tag>;
    }
  }
}
