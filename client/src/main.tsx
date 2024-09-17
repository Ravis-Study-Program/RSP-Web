import '@mantine/core/styles.css';
import '@mantine/charts/styles.css';
import '@mantine/dates/styles.css';
import 'mantine-react-table/styles.css';

import React from 'react';
import { Auth0Provider } from '@auth0/auth0-react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import ReactDOM from 'react-dom/client';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { MantineProvider } from '@mantine/core';
import { ModalsProvider } from '@mantine/modals';
import routes from './Router';
import { theme } from './theme';

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <MantineProvider theme={theme}>
      <QueryClientProvider client={queryClient}>
        <ModalsProvider>
          <Auth0Provider
            domain={import.meta.env.VITE_APP_AUTH0_DOMAIN as string}
            clientId={import.meta.env.VITE_APP_AUTH0_CLIENT_ID as string}
            authorizationParams={{
              redirect_uri: import.meta.env.VITE_APP_AUTH0_REDIRECT_URI as string,
              audience: import.meta.env.VITE_APP_AUTH0_AUDIENCE as string,
            }}
          >
            <RouterProvider router={createBrowserRouter(routes(queryClient))} />
          </Auth0Provider>
        </ModalsProvider>
      </QueryClientProvider>
    </MantineProvider>
  </React.StrictMode>
);
