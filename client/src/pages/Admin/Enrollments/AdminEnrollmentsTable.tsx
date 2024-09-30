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
  EnrollmentResponse,
  useAdminCreateEnrollment,
  useAdminDeleteEnrollment,
  useAdminListEnrollment,
  useAdminListRole,
  useAdminListSeason,
  useAdminListUser,
  useAdminUpdateEnrollment,
} from '@/generated/api/client';
import { AdminEnrollmentsCreateModal } from './AdminEnrollmentsCreateModal';
import { AdminEnrollmentsUpdateModal } from './AdminEnrollmentsUpdateModal';
import classes from './AdminEnrollmentsTable.module.css';

export const AdminEnrollmentsTable = () => {
  const {
    data: enrollmentResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
    refetch: refetchEnrollments,
  } = useAdminListEnrollment();
  const { mutateAsync: createEnrollment, status: isCreatingEnrollmentStatus } =
    useAdminCreateEnrollment();
  const { mutateAsync: updateEnrollment, status: isUpdatingEnrollmentStatus } =
    useAdminUpdateEnrollment();
  const { mutateAsync: deleteEnrollment, status: isDeletingEnrollmentStatus } =
    useAdminDeleteEnrollment();
  const {
    data: seasonResponse,
    isError: isLoadingSeasonsError,
    isFetching: isFetchingSeasons,
    isLoading: isLoadingSeasons,
  } = useAdminListSeason();
  const {
    data: roleResponse,
    isError: isLoadingRolesError,
    isFetching: isFetchingRoles,
    isLoading: isLoadingRoles,
  } = useAdminListRole();
  const {
    data: userResponse,
    isError: isLoadingUsersError,
    isFetching: isFetchingUsers,
    isLoading: isLoadingUsers,
  } = useAdminListUser();

  const openDeleteConfirmModal = (row: MRT_Row<EnrollmentResponse>) => {
    modals.openConfirmModal({
      title: 'Delete Enrollment',
      children: (
        <Text>Are you sure you want to delete this enrollment? This action cannot be undone.</Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await deleteEnrollment({ params: { enrollmentId: row.original.enrollmentId! } });
        await refetchEnrollments();
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<EnrollmentResponse>[]>(
    () => [
      {
        accessorKey: 'season',
        header: 'Season',
      },
      {
        accessorKey: 'user',
        header: 'User',
      },
      {
        accessorKey: 'role',
        header: 'Role',
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: enrollmentResponse?.responseBody?.enrollments ?? [],
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
          id: 'season',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.enrollmentId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingEnrollmentsError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminEnrollmentsCreateModal
        table={table}
        createEnrollment={createEnrollment}
        refetchEnrollments={refetchEnrollments}
        seasons={seasonResponse?.responseBody?.seasons}
        roles={roleResponse?.responseBody?.roles}
        users={userResponse?.responseBody?.users}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminEnrollmentsUpdateModal
        table={table}
        row={row}
        updateEnrollment={updateEnrollment}
        refetchEnrollments={refetchEnrollments}
        seasons={seasonResponse?.responseBody?.seasons}
        roles={roleResponse?.responseBody?.roles}
        users={userResponse?.responseBody?.users}
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
        Create New Enrollment
      </Button>
    ),
    state: {
      isLoading: isLoadingEnrollments || isLoadingRoles || isLoadingSeasons || isLoadingUsers,
      isSaving:
        isCreatingEnrollmentStatus === 'pending' ||
        isUpdatingEnrollmentStatus === 'pending' ||
        isDeletingEnrollmentStatus === 'pending',
      showAlertBanner:
        isLoadingEnrollmentsError ||
        isLoadingRolesError ||
        isLoadingSeasonsError ||
        isLoadingUsersError,
      showProgressBars:
        isFetchingEnrollments || isFetchingRoles || isFetchingSeasons || isFetchingUsers,
    },
  });

  return <MantineReactTable table={table} />;
};
