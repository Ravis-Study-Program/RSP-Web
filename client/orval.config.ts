import * as dotenv from 'dotenv';
import { defineConfig } from 'orval';
import { customTransformer } from './src/shared/api/Transformer';

dotenv.config();

const serverUrl = process.env.VITE_APP_SERVER_URL;
const baseUrl = process.env.VITE_APP_BASE_API_URL;

export default defineConfig({
  api: {
    input: {
      target: `${serverUrl}/swagger/v1/swagger.json`,
      override: {
        transformer: customTransformer,
      },
    },
    output: {
      mode: 'single',
      clean: true,
      target: 'src/generated/api',
      client: 'react-query',
      mock: false,
      baseUrl,
      override: {
        title() {
          return 'title';
        },
        mutator: {
          path: 'src/shared/api/AxiosCustomInstance.ts',
          name: 'CustomAxiosInstance',
        },
        query: {
          useQuery: true,
          options: {
            staleTime: Infinity,
          },
          signal: true,
        },
      },
    },
    hooks: {
      afterAllFilesWrite: 'prettier --write',
    },
  },
});
