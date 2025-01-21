import { useMemo } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Button, Flex, Text, Title, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  AdminDeleteEnrollmentResponseApiResponse,
  EnrollmentEntity,
  EnrollmentResponseDto,
  SeasonRole,
  SeasonStudentRolePromotion,
  useAdminCreateEnrollment,
  useAdminDeleteEnrollment,
  useAdminListEnrollment,
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
    data: userResponse,
    isError: isLoadingUsersError,
    isFetching: isFetchingUsers,
    isLoading: isLoadingUsers,
  } = useAdminListUser();

  const openDeleteConfirmModal = (row: MRT_Row<EnrollmentResponseDto>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            Delete Enrollment
          </Title>
          <Text>
            Are you sure you want to delete this enrollment? This action cannot be undone.
          </Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteEnrollment({ data: { enrollmentId: row.original.enrollmentId! } });
          await refetchEnrollments();
          modals.closeAll();
          notifications.show({
            color: 'green',
            title: 'Success',
            message: 'Enrollment deleted successfully.',
          });
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteEnrollmentResponseApiResponse;
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

  const columns = useMemo<MRT_ColumnDef<EnrollmentResponseDto>[]>(
    () => [
      {
        accessorKey: 'seasonName',
        header: 'Season',
      },
      {
        accessorKey: 'userName',
        header: 'User',
      },
      {
        header: 'Role',
        accessorFn: (row) => {
          const roleKey = Object.keys(SeasonRole).find(
            (key) => SeasonRole[key as keyof typeof SeasonRole] === row.role
          ) as keyof typeof SeasonRole;

          return roleKey ? roleKey : 'Unknown Role';
        },
      },
      {
        header: 'Student Role Promotion',
        accessorFn: (row) => {
          const roleKey = Object.keys(SeasonStudentRolePromotion).find(
            (key) =>
              SeasonStudentRolePromotion[key as keyof typeof SeasonStudentRolePromotion] ===
              row.studentRolePromotion
          ) as keyof typeof SeasonStudentRolePromotion;

          return roleKey ? roleKey : 'Unknown Role';
        },
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
          id: 'seasonName',
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
      isLoading: isLoadingEnrollments || isLoadingSeasons || isLoadingUsers,
      isSaving:
        isCreatingEnrollmentStatus === 'pending' ||
        isUpdatingEnrollmentStatus === 'pending' ||
        isDeletingEnrollmentStatus === 'pending',
      showAlertBanner: isLoadingEnrollmentsError || isLoadingSeasonsError || isLoadingUsersError,
      showProgressBars: isFetchingEnrollments || isFetchingSeasons || isFetchingUsers,
    },
  });

  return <MantineReactTable table={table} />;
};
