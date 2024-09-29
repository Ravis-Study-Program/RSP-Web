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
  Role,
  useAdminCreateRole,
  useAdminDeleteRole,
  useAdminListRole,
  useAdminUpdateRole,
} from '@/generated/api/client';
import { AdminRolesCreateModal } from './AdminRolesCreateModal';
import { AdminRolesUpdateModal } from './AdminRolesUpdateModal';
import classes from './AdminRolesTable.module.css';

export const AdminRolesTable = () => {
  const {
    data: roleResponse,
    isError: isLoadingRolesError,
    isFetching: isFetchingRoles,
    isLoading: isLoadingRoles,
    refetch: refetchRoles,
  } = useAdminListRole();
  const { mutateAsync: createRole, status: isCreatingRoleStatus } = useAdminCreateRole();
  const { mutateAsync: updateRole, status: isUpdatingRoleStatus } = useAdminUpdateRole();
  const { mutateAsync: deleteRole, status: isDeletingRoleStatus } = useAdminDeleteRole();

  const openDeleteConfirmModal = (row: MRT_Row<Role>) => {
    modals.openConfirmModal({
      title: 'Delete Role',
      children: (
        <Text>Are you sure you want to delete this role? This action cannot be undone.</Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await deleteRole({ params: { roleId: row.original.roleId! } });
        await refetchRoles();
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<Role>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: roleResponse?.responseBody?.roles ?? [],
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
          id: 'name',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.roleId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingRolesError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminRolesCreateModal table={table} createRole={createRole} refetchRoles={refetchRoles} />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminRolesUpdateModal
        table={table}
        row={row}
        updateRole={updateRole}
        refetchRoles={refetchRoles}
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
        Create New Role
      </Button>
    ),
    state: {
      isLoading: isLoadingRoles,
      isSaving:
        isCreatingRoleStatus === 'pending' ||
        isUpdatingRoleStatus === 'pending' ||
        isDeletingRoleStatus === 'pending',
      showAlertBanner: isLoadingRolesError,
      showProgressBars: isFetchingRoles,
    },
  });

  return <MantineReactTable table={table} />;
};
