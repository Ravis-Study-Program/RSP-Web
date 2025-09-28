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
  AdminDeleteMentorshipResponseApiResponse,
  MentorshipResponse,
  useAdminCreateMentorship,
  useAdminDeleteMentorship,
  useAdminListEnrollment,
  useAdminListMentorship,
  useAdminListSeason,
  useAdminUpdateMentorship,
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
import { AdminMentorshipsCreateModal } from './AdminMentorshipsCreateModal';
import { AdminMentorshipsUpdateModal } from './AdminMentorshipsUpdateModal';

export const AdminMentorshipsTable = () => {
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
    data: mentorshipResponse,
    isError: isLoadingMentorshipsError,
    isFetching: isFetchingMentorships,
    isLoading: isLoadingMentorships,
    refetch: refetchMentorships,
  } = useAdminListMentorship();
  const { mutateAsync: createMentorship, status: isCreatingMentorshipStatus } =
    useAdminCreateMentorship();
  const { mutateAsync: updateMentorship, status: isUpdatingMentorshipStatus } =
    useAdminUpdateMentorship();
  const { mutateAsync: deleteMentorship, status: isDeletingMentorshipStatus } =
    useAdminDeleteMentorship();
  const {
    data: seasonResponse,
    isError: isLoadingSeasonsError,
    isFetching: isFetchingSeasons,
    isLoading: isLoadingSeasons,
  } = useAdminListSeason();
  const {
    data: enrollmentResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useAdminListEnrollment();

  const openDeleteConfirmModal = (row: MRT_Row<MentorshipResponse>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            {CONFIRMATION_MESSAGES.MENTORSHIP.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.MENTORSHIP.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await deleteMentorship({ data: { mentorshipId: row.original.mentorshipId! } });
          await refetchMentorships();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.MENTORSHIP.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteMentorshipResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
        }
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<MentorshipResponse>[]>(
    () => [
      {
        header: 'Season',
        accessorFn: (row) => row.seasonName,
      },
      {
        header: 'Mentor',
        accessorFn: (row) => row.mentorName,
      },
      {
        header: 'Mentee',
        accessorFn: (row) => row.menteeName,
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: mentorshipResponse?.responseBody?.mentorships ?? [],
    ...getMantineTablePropsWithBanner(
      classes.table,
      isLoadingMentorshipsError || isLoadingEnrollmentsError || isLoadingSeasonsError
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
          id: 'Season',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.mentorshipId?.toString(),
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminMentorshipsCreateModal
        table={table}
        createMentorship={createMentorship}
        refetchMentorships={refetchMentorships}
        seasons={seasonResponse?.responseBody?.seasons}
        enrollments={enrollmentResponse?.responseBody?.enrollments}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminMentorshipsUpdateModal
        table={table}
        row={row}
        updateMentorship={updateMentorship}
        refetchMentorships={refetchMentorships}
        seasons={seasonResponse?.responseBody?.seasons}
        enrollments={enrollmentResponse?.responseBody?.enrollments}
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
        Create New Mentorship
      </Button>
    ),
    state: {
      pagination,
      isLoading: isLoadingMentorships || isLoadingEnrollments || isLoadingSeasons,
      isSaving:
        isCreatingMentorshipStatus === 'pending' ||
        isUpdatingMentorshipStatus === 'pending' ||
        isDeletingMentorshipStatus === 'pending',
      showAlertBanner:
        isLoadingMentorshipsError || isLoadingEnrollmentsError || isLoadingSeasonsError,
      showProgressBars: isFetchingMentorships || isFetchingEnrollments || isFetchingSeasons,
    },
  });

  return <MantineReactTable table={table} />;
};
