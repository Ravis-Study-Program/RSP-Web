import { createTheme } from '@mantine/core';

export const theme = createTheme({
  primaryColor: 'blue',
  breakpoints: {
    xs: '30em',
    sm: '48em',
    md: '64em',
    lg: '74em',
    xl: '90em',
  },
  components: {
    Modal: {
      styles: {
        header: {
          minHeight: 0,
          padding: 0,
          paddingTop: 'var(--mantine-spacing-md)',
        },
        close: {
          position: 'absolute',
          top: 'var(--mantine-spacing-md)',
          right: 'var(--mantine-spacing-md)',
        },
      },
    },
  },
});
