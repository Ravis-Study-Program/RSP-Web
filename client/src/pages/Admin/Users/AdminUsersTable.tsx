import { useMemo, useState } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Button, Flex, Text, Title, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  AdminDeleteUserResponseApiResponse,
  AdminUserDto,
  useAdminCreateUser,
  useAdminDeleteUser,
  useAdminListUser,
  useAdminUpdateUser,
} from '@/generated/api/client';
import {
  getConfirmModalProps,
  getErrorNotification,
  getMantineTablePropsWithBanner,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';
import { CONFIRMATION_MESSAGES } from '@/shared/constants/messages';
import classes from '@/shared/styles/tableStyles.module.css';
import { AdminUsersCreateModal } from './AdminUsersCreateModal';
import { AdminUsersUpdateModal } from './AdminUsersUpdateModal';

export const AdminUsersTable = () => {
  const [pageSize, setPageSize] = useLocalStorage({
    key: 'page-size',
    defaultValue: 10,
    getInitialValueInEffect: false,
  });

  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize,
  });

  const {
    data: userResponse,
    isError: isLoadingUsersError,
    isFetching: isFetchingUsers,
    isLoading: isLoadingUsers,
    refetch: refetchUsers,
  } = useAdminListUser();
  const { mutateAsync: createUser, status: isCreatingUserStatus } = useAdminCreateUser();
  const { mutateAsync: updateUser, status: isUpdatingUserStatus } = useAdminUpdateUser();
  const { mutateAsync: deleteUser, status: isDeletingUserStatus } = useAdminDeleteUser();

  const openDeleteConfirmModal = (row: MRT_Row<AdminUserDto>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            {CONFIRMATION_MESSAGES.USER.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.USER.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await deleteUser({ data: { userId: row.original.userId } });
          await refetchUsers();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.USER.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteUserResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
        }
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<AdminUserDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
      },
      {
        accessorKey: 'email',
        header: 'Email',
      },
      {
        accessorKey: 'isAdmin',
        header: 'Is Admin',
        accessorFn: (row) => (row.isAdmin ? 'Yes' : 'No'),
        Cell: ({ row }) => (row.original.isAdmin ? 'Yes' : 'No'),
      },
      {
        accessorKey: 'isTestUser',
        header: 'Is Test User',
        accessorFn: (row) => (row.isTestUser ? 'Yes' : 'No'),
        Cell: ({ row }) => (row.original.isTestUser ? 'Yes' : 'No'),
      },
      {
        accessorKey: 'discordId',
        header: 'Discord ID',
      },
      {
        accessorKey: 'profileImage',
        header: 'Profile Image',
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: userResponse?.responseBody?.users ?? [],
    ...getMantineTablePropsWithBanner(classes.table, isLoadingUsersError),
    createDisplayMode: 'modal',
    onPaginationChange: (updater) => {
      const next = typeof updater === 'function' ? updater(pagination) : updater;
      setPageSize(next.pageSize);
      setPagination(next);
    },
    editDisplayMode: 'modal',
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'name',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.userId?.toString(),
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminUsersCreateModal table={table} createUser={createUser} refetchUsers={refetchUsers} />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminUsersUpdateModal
        table={table}
        row={row}
        updateUser={updateUser}
        refetchUsers={refetchUsers}
      />
    ),
    renderRowActions: ({ row, table }) => (
      <Flex gap="md">
        <Tooltip label="Edit">
          <ActionIcon variant="subtle" onClick={() => table.setEditingRow(row)}>
            <IconEdit />
          </ActionIcon>
        </Tooltip>
        <Tooltip label="Delete">
          <ActionIcon variant="subtle" color="red" onClick={() => openDeleteConfirmModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    renderTopToolbarCustomActions: ({ table }) => (
      <Button
        onClick={() => {
          table.setCreatingRow(true);
        }}
      >
        Create New User
      </Button>
    ),
    state: {
      pagination,
      isLoading: isLoadingUsers,
      isSaving:
        isCreatingUserStatus === 'pending' ||
        isUpdatingUserStatus === 'pending' ||
        isDeletingUserStatus === 'pending',
      showAlertBanner: isLoadingUsersError,
      showProgressBars: isFetchingUsers,
    },
  });

  return <MantineReactTable table={table} />;
};
