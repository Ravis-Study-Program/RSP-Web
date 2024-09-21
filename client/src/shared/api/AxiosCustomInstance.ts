import Axios, { AxiosRequestConfig } from 'axios';
import { auth0Client } from '../auth/Auth0Client';

export const CustomAxiosInstance = async <T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig
): Promise<T> => {
  const token = await auth0Client.getTokenSilently();
  const source = Axios.CancelToken.source();

  const headers = {
    ...config.headers,
    Authorization: token ? `Bearer ${token}` : undefined,
  };

  const promise = Axios({
    ...config,
    ...options,
    headers,
    cancelToken: source.token,
  }).then(({ data }) => data);

  return promise;
};
