import apiService from './axiosInstance';
import { notification } from 'antd';
import { FeatureFlag } from '../models/featureFlag';

export default class FeatureFlagService {
  public static async GetAll(): Promise<FeatureFlag[]> {
    try {
      const { data } = await apiService.get('/thoth-api/FeatureFlag');
      return await Promise.resolve(data);
    } catch {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
      return Promise.reject();
    }
  }

  public static async Delete(name: string): Promise<boolean> {
    try {
      const { status } = await apiService.delete(`/thoth-api/FeatureFlag/${name}`);

      return status < 400;
    } catch {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
      return false;
    }
  }

  public static async Update(featureFlag: FeatureFlag): Promise<boolean> {
    console.log(featureFlag);
    try {
      const { status } = await apiService.put(
        `/thoth-api/FeatureFlag/${featureFlag.name}`,
        featureFlag
      );

      return status < 400;
    } catch {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
      return false;
    }
  }
}
