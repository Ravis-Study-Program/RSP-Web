import Axios, { AxiosRequestConfig } from 'axios';
import qs from 'qs';
import { auth0Client } from '../auth/Auth0Client';

export const CustomAxiosInstance = async <T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig
): Promise<T> => {
  try {
    const token = await auth0Client.getTokenSilently();
    const source = Axios.CancelToken.source();

    const headers = {
      ...config.headers,
      Authorization: token ? `Bearer ${token}` : undefined,
    };

    // Merge the configurations and add the paramsSerializer option.
    const mergedConfig: AxiosRequestConfig = {
      ...config,
      ...options,
      headers,
      cancelToken: source.token,
      paramsSerializer: (params) => qs.stringify(params, { arrayFormat: 'repeat' }),
    };

    const promise = Axios(mergedConfig).then(({ data }) => data);

    return promise;
  } catch (err) {
    // eslint-disable-next-line no-console
    console.error('Error in CustomAxiosInstance:', err);
    throw err;
  }
};
