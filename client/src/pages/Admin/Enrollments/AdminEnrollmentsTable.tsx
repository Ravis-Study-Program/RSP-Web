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
import { ProfileLink } from '@/components/ProfileLink/ProfileLink';
import {
  AdminDeleteEnrollmentResponseApiResponse,
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
import {
  getConfirmModalProps,
  getErrorNotification,
  getMantineTablePropsWithBanner,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';
import { CONFIRMATION_MESSAGES } from '@/shared/constants/messages';
import classes from '@/shared/styles/tableStyles.module.css';
import { AdminEnrollmentsCreateModal } from './AdminEnrollmentsCreateModal';
import { AdminEnrollmentsUpdateModal } from './AdminEnrollmentsUpdateModal';

export const AdminEnrollmentsTable = () => {
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
            {CONFIRMATION_MESSAGES.ENROLLMENT.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.ENROLLMENT.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await deleteEnrollment({ data: { enrollmentId: row.original.enrollmentId! } });
          await refetchEnrollments();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.ENROLLMENT.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteEnrollmentResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
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
        Cell: ({ row }) => {
          return <ProfileLink userName={row.original.userName} userSlug={row.original.userSlug} />;
        },
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
    ...getMantineTablePropsWithBanner(
      classes.table,
      isLoadingEnrollmentsError || isLoadingSeasonsError || isLoadingUsersError
    ),
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
          id: 'seasonName',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.enrollmentId?.toString(),
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
      pagination,
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
