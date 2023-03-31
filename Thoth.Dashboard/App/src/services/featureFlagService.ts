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
}
