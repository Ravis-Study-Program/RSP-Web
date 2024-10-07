import { defineConfig } from 'orval';

const serverUrl = 'http://localhost:4000';

export default defineConfig({
  api: {
    input: {
      target: `${serverUrl}/swagger/v1/swagger.json`,
    },
    output: {
      mode: 'single',
      clean: true,
      target: 'src/generated/api',
      client: 'react-query',
      mock: false,
      baseUrl: serverUrl,
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
            staleTime: 8000, // TODO: need to tweak this properly
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
