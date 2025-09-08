import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  plugins: [react(), tsconfigPaths()],
  define: {
    'process.env.VITE_APP_AUTH0_DOMAIN': JSON.stringify(process.env.VITE_APP_AUTH0_DOMAIN),
    'process.env.VITE_APP_AUTH0_CLIENT_ID': JSON.stringify(process.env.VITE_APP_AUTH0_CLIENT_ID),
    'process.env.VITE_APP_AUTH0_REDIRECT_URI': JSON.stringify(process.env.VITE_APP_AUTH0_REDIRECT_URI),
    'process.env.VITE_APP_AUTH0_AUDIENCE': JSON.stringify(process.env.VITE_APP_AUTH0_AUDIENCE),
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './vitest.setup.mjs',
  },
  server: {
    port: 8080,
    host: '0.0.0.0',
    strictPort: true,
    allowedHosts: ['client'],
  },
  preview: {
    port: 80,
    host: '0.0.0.0',
    strictPort: true
  },
  resolve: {
    alias: {
      // /esm/icons/index.mjs only exports the icons statically, so no separate chunks are created
      '@tabler/icons-react': '@tabler/icons-react/dist/esm/icons/index.mjs',
    },
  },
});
