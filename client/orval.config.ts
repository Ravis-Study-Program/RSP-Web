import { defineConfig } from 'orval';

const serverUrl = 'http://localhost:4000';

export default defineConfig({
  petstore: {
    input: {
      target: `${serverUrl}/swagger/v1/swagger.json`,
    },
    output: {
      mode: 'single',
      target: 'src/generated/actions',
      schemas: 'src/generated/models',
      client: 'react-query',
      mock: true,
      baseUrl: serverUrl,
      override: {
        title() {
          return 'title';
        },
        mutator: {
          path: 'src/shared/api/AxiosCustomInstance.ts',
          name: 'CustomAxiosInstance',
        },
      },
    },
    hooks: {
      afterAllFilesWrite: 'prettier --write',
    },
  },
});
