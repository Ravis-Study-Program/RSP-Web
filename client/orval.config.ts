import { defineConfig } from 'orval';

const serverUrl = process.env.VITE_APP_AUTH0_DOMAIN;

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
