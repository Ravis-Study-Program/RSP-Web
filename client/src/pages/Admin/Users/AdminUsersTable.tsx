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
  useAdminCreateUser,
  useAdminDeleteUser,
  useAdminListUser,
  useAdminUpdateUser,
  UserEntity,
} from '@/generated/api/client';
import { AdminUsersCreateModal } from './AdminUsersCreateModal';
import { AdminUsersUpdateModal } from './AdminUsersUpdateModal';
import classes from './AdminUsersTable.module.css';

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

  const openDeleteConfirmModal = (row: MRT_Row<UserEntity>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            Delete User
          </Title>
          <Text>Are you sure you want to delete this user? This action cannot be undone.</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteUser({ data: { email: row.original.email } });
          await refetchUsers();
          modals.closeAll();
          notifications.show({
            color: 'green',
            title: 'Success',
            message: 'User deleted successfully.',
          });
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteUserResponseApiResponse;
          notifications.show({
            color: 'red',
            title: 'Error',
            autoClose: false,
            message: response.error?.message,
          });
        }
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<UserEntity>[]>(
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
