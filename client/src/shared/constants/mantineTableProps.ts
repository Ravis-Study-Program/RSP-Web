export const getMantineModalProps = () => ({
  mantineCreateRowModalProps: {
    closeOnClickOutside: false,
    withCloseButton: true,
    size: 'lg',
    padding: 'xl',
  } as const,
  mantineEditRowModalProps: {
    closeOnClickOutside: false,
    withCloseButton: true,
    size: 'lg',
    padding: 'xl',
  } as const,
});

export const getMantinePaperProps = (tableClassName: string) => ({
  mantinePaperProps: {
    className: tableClassName,
  },
});

export const getMantineToolbarAlertBannerProps = (hasError: boolean) => ({
  mantineToolbarAlertBannerProps: hasError
    ? {
        color: 'red',
        children: 'Error loading data',
      }
    : undefined,
});

export const getMantineTableProps = (tableClassName: string) => ({
  ...getMantinePaperProps(tableClassName),
  ...getMantineModalProps(),
});

export const getMantineTablePropsWithBanner = (tableClassName: string, hasError: boolean) => ({
  ...getMantineTableProps(tableClassName),
  ...getMantineToolbarAlertBannerProps(hasError),
});

export const getConfirmModalProps = () => ({
  confirmProps: { color: 'red' } as const,
});

export const getSuccessNotification = (message: string) => ({
  color: 'green',
  title: 'Success',
  message,
});

export const getErrorNotification = (message: string | undefined | null) => ({
  color: 'red',
  title: 'Error',
  autoClose: false,
  message: message || 'An error occurred',
});

export { NOTIFICATION_MESSAGES } from './messages';
