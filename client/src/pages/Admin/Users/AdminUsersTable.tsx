import { useMemo } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Button, Flex, Text, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import {
  useAdminCreateUser,
  useAdminDeleteUser,
  useAdminListUser,
  useAdminUpdateUser,
  User,
} from '@/generated/api/client';
import { AdminUsersCreateModal } from './AdminUsersCreateModal';
import { AdminUsersUpdateModal } from './AdminUsersUpdateModal';
import classes from './AdminUsersTable.module.css';

export const AdminUsersTable = () => {
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

  const openDeleteConfirmModal = (row: MRT_Row<User>) => {
    modals.openConfirmModal({
      title: 'Delete User',
      children: (
        <Text>Are you sure you want to delete this user? This action cannot be undone.</Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await deleteUser({ params: { email: row.original.email } });
        await refetchUsers();
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<User>[]>(
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
        Cell: ({ row }) => {
          return <Text size="sm">{row.original.isAdmin ? 'Yes' : 'No'}</Text>;
        },
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
    mantinePaperProps: {
      className: classes.table,
    },
    createDisplayMode: 'modal',
    mantineCreateRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    mantineEditRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    editDisplayMode: 'modal',
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'email',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.userId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingUsersError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
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
