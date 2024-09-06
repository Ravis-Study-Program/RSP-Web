import '@mantine/core/styles.css';

import { Auth0Provider } from '@auth0/auth0-react';
import { MantineProvider } from '@mantine/core';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from "react-query";
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import routes from "./Router";
import { theme } from './theme';

const queryClient = new QueryClient();

<React.StrictMode>
<MantineProvider theme={theme}>
  <RouterProvider router={createBrowserRouter(routes(queryClient))} />
</MantineProvider>
</React.StrictMode>

ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
        <MantineProvider theme={theme}>
        <QueryClientProvider client={queryClient}>
            
                <Auth0Provider
                    domain={import.meta.env.VITE_APP_AUTH0_DOMAIN as string}
                    clientId={import.meta.env.VITE_APP_AUTH0_CLIENT_ID as string}
                    authorizationParams={{
                        redirect_uri: window.location.origin,
                        audience: import.meta.env.VITE_APP_AUTH0_AUDIENCE as string,
                    }}
                >
  <RouterProvider router={createBrowserRouter(routes(queryClient))} />
                </Auth0Provider>
        </QueryClientProvider>
            </MantineProvider>
    </React.StrictMode>
);

