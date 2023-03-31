import axios from 'axios';
import { notification } from 'antd';

const apiService = axios.create();

apiService.interceptors.response.use(
  async (response) => {
    return response;
  },
  (error) => {
    if (error?.response.status >= 400) {
      notification.error({
        message: 'We failed you...',
        description: 'We could not retrieve the information you requested, please try again.',
      });
    }

    return error;
  }
);

export default apiService;
