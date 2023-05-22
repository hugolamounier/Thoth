import { Moment } from 'moment';

export type FeatureManager = {
  name: string;
  type: FeatureTypes;
  subType?: FeatureFlagsTypes;
  enabled: boolean;
  value?: string;
  description?: string;
  histories?: FeatureManagerHistory[];
  extras?: string;
  createdAt: Moment;
  updatedAt?: Moment;
  deletedAt?: Moment;
};

export type FeatureManagerHistory = FeatureManager & {
  periodStart?: Moment;
  periodEnd?: Moment;
};

export enum FeatureFlagsTypes {
  Boolean = 1,
  PercentageFilter = 2,
}

export enum FeatureTypes {
  EnvironmentVariable = 1,
  FeatureFlag = 2,
}
