import { Moment } from 'moment';

export type FeatureManager = {
  name: string;
  type: FeatureTypes;
  subType?: FeatureFlagsTypes;
  enabled: boolean;
  value?: string;
  description?: string;
  createdAt: Moment;
  updatedAt?: Moment;
};

export enum FeatureFlagsTypes {
  Boolean = 1,
  PercentageFilter = 2,
}

export enum FeatureTypes {
  EnvironmentVariable = 1,
  FeatureFlag = 2,
}
