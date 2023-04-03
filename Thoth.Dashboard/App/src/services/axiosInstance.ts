import axios from 'axios';
import { notification } from 'antd';

const apiService = axios.create();

apiService.interceptors.response.use(
  async (response) => {
    return response;
  },
  (error) => {
    console.log(error);
    if (error?.response.status >= 400) {
      notification.error({
        message: 'Something is wrong...',
        description: error?.response.data,
      });
    }

    return error;
  }
);

export default apiService;
