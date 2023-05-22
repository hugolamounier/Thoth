import apiService from './axiosInstance';
import { notification } from 'antd';
import { FeatureManager } from '../models/featureManager';

export default class FeatureFlagService {
  public static async GetAll(): Promise<FeatureManager[]> {
    try {
      const { data } = await apiService.get('/thoth-api/FeatureFlag');
      return data;
    } catch {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
      return Promise.reject();
    }
  }

  public static async GetAllDeleted(): Promise<FeatureManager[]> {
    try {
      const { data } = await apiService.get('/thoth-api/FeatureFlag/Deleted');
      return data;
    } catch {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
      return Promise.reject();
    }
  }

  public static async Create(featureFlag: FeatureManager): Promise<boolean> {
    try {
      const { status } = await apiService.post(`/thoth-api/FeatureFlag`, featureFlag);

      return status < 400;
    } catch {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
      return false;
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

  public static async Update(featureFlag: FeatureManager): Promise<boolean> {
    try {
      const { status } = await apiService.put(`/thoth-api/FeatureFlag`, featureFlag);

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
