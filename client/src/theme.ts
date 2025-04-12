import { createTheme } from '@mantine/core';

export const theme = createTheme({
  fontFamily:
    'Inter, -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Helvetica, Arial, sans-serif, Apple Color Emoji, Segoe UI Emoji',
  primaryColor: 'blue',
  breakpoints: {
    xs: '30em',
    sm: '48em',
    md: '64em',
    lg: '74em',
    xl: '90em',
  },
  cursorType: 'pointer',
  components: {
    Modal: {
      styles: {
        close: {
          position: 'absolute',
          top: 'var(--mantine-spacing-md)',
          right: 'var(--mantine-spacing-md)',
        },
        header: {
          position: 'absolute',
          width: '100%',
          background: 'none',
        },
      },
    },
  },
});
