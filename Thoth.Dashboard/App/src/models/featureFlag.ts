import { Moment } from 'moment';

export type FeatureFlag = {
  name: string;
  type: FeatureFlagsTypes;
  value: boolean;
  filterValue?: string;
  createdAt: Moment;
  updatedAt?: Moment;
};

export enum FeatureFlagsTypes {
  Boolean = 2,
}
